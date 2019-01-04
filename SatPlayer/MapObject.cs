using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
using System;
using System.Collections.Generic;
using System.Linq;
using SatIO;
using BaseComponent;
using System.Collections.Concurrent;

namespace SatPlayer
{
    public class MapObject : MultiAnimationObject2D, IEffectManeger, ICloneable, IMapObjectData, IDamageControler
    {
        static ScriptOptions options = ScriptOptions.Default.WithImports("SatPlayer", "PhysicAltseed", "System", "System.Collections.Generic")
                                         .WithReferences(System.Reflection.Assembly.GetAssembly(typeof(IEnumerator<>))
                                                         , System.Reflection.Assembly.GetAssembly(typeof(MapObject))
                                                         , System.Reflection.Assembly.GetAssembly(typeof(MapObjectType))
                                                         , System.Reflection.Assembly.GetAssembly(typeof(asd.Vector2DF))
                                                         , System.Reflection.Assembly.GetAssembly(typeof(PhysicalRectangleShape)));

        public new asd.Vector2DF Position
        {
            get
            {
                return base.Position;
            }

            set
            {
                base.Position = value;
                if (MapObjectType == MapObjectType.Active) CollisionShape.DrawingArea = new asd.RectF(value - CollisionShape.CenterPosition, CollisionShape.DrawingArea.Size);
                else collisionShape.DrawingArea = new asd.RectF(value - collisionShape.DrawingArea.Size / 2, collisionShape.DrawingArea.Size);
            }
        }

        public new string State
        {
            get => base.State;
            set
            {
                base.State = value;
                IsOneLoop = false;
            }
        }

        public string GroupName { get; set; }

        public MapObjectType MapObjectType { get; private set; }

        asd.RectangleShape collisionShape;
        public PhysicalRectangleShape CollisionShape { get => collisionShape as PhysicalRectangleShape; }

        public Dictionary<string, Effect> Effects { get; protected set; }

        public MainMapLayer2D RefMainMapLayer2D => Layer as MainMapLayer2D;

        PhysicalShape IMapObjectData.CollisionShape => collisionShape as PhysicalRectangleShape;

        public Action<MapObject> Update { get; set; }

        public int HP
        {
            get => hP;
            set
            {
                hP = value;
                //TODO: ダメージ食らった時のイベント
                if (HP < 0) Dispose();
            }
        }
        public bool IsReceiveDamage { get; set; }

        public Queue<DamageRect> DamageRequests { get; private set; }

        public DamageRect.OwnerType OwnerType { get; set; }

        asd.Shape IDamageControler.CollisionShape => collisionShape;

        public Queue<DirectDamage> DirectDamageRequests { get; private set; }

        protected Dictionary<string, Sensor> sensors;
        protected Dictionary<string, Sound> sounds;
        protected Dictionary<string, MapObject> childMapObjectData;
        protected PhysicalWorld refWorld;
        private int hP;

        BlockingCollection<Action> subQueue;
        BlockingCollection<Action> mainQueue;

        public MapObject(BlockingCollection<Action> subThreadQueue, BlockingCollection<Action> mainThreadQueue, string scriptPath, PhysicalWorld world)
        {
            CameraGroup = 1;
            sensors = new Dictionary<string, Sensor>();
            childMapObjectData = new Dictionary<string, MapObject>();
            sounds = new Dictionary<string, Sound>();
            Effects = new Dictionary<string, Effect>();
            Update = (obj) => { };
            refWorld = world;
            DamageRequests = new Queue<DamageRect>();
            DirectDamageRequests = new Queue<DirectDamage>();
            collisionShape = new asd.RectangleShape();
            MapObjectType = MapObjectType.Passive;
            subQueue = subThreadQueue;
            mainQueue = mainThreadQueue;
            Script<object> script;
            subThreadQueue.TryAdd(() =>
            {
                if (scriptPath != "")
                {
                    try
                    {
                        script = CSharpScript.Create(IO.GetStream(scriptPath), options: options, globalsType: this.GetType());
                        script.Compile();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    mainThreadQueue.Add(() =>
                    {
                        var thread = script.RunAsync(this);
                        thread.Wait();
                    });
                }
                mainThreadQueue.TryAdd(() =>
                {
                    try
                    {
                        collisionShape.DrawingArea = new asd.RectF(new asd.Vector2DF(), AnimationPart.First().Value.Textures.First().Size.To2DF());
                    }
                    catch (Exception e)
                    {
                        ErrorIO.AddError(e);
                    }
                    CenterPosition = collisionShape.DrawingArea.Size / 2;
                    Position = Position;
                });
            });
        }

        protected MapObject()
        {
            CameraGroup = 1;
            DamageRequests = new Queue<DamageRect>();
            sensors = new Dictionary<string, Sensor>();
            Effects = new Dictionary<string, Effect>();
            childMapObjectData = new Dictionary<string, MapObject>();
            sounds = new Dictionary<string, Sound>();
            Update = (obj) => { };
            collisionShape = new asd.RectangleShape();
            MapObjectType = MapObjectType.Passive;
        }

        protected override void OnAdded()
        {
            Player = (Layer as MainMapLayer2D)?.Player;
            base.OnAdded();
        }

        protected override void OnDispose()
        {
            Update = (obj) => { };
            if (MapObjectType == MapObjectType.Active) CollisionShape.Dispose();
            base.OnDispose();
        }

        protected override void OnUpdate()
        {
            if (MapObjectType == MapObjectType.Active)
            {
                base.Position = CollisionShape.CenterPosition + CollisionShape.DrawingArea.Position;
                if (Math.Abs(CollisionShape.Angle) > 1.0f) CollisionShape.AngularVelocity = -CollisionShape.Angle * 30.0f;
            }

            foreach (var item in sensors) item.Value.Update(this);

            Update(this);

            base.OnUpdate();
        }

        public void SetType(MapObjectType objectType)
        {
            MapObjectType = objectType;
            switch (objectType)
            {
                case MapObjectType.Active:
                    collisionShape = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, refWorld);
                    DrawingPriority = 2;
                    IsReceiveDamage = true;
                    HP = 100;
                    break;
                case MapObjectType.Passive:
                    collisionShape = new asd.RectangleShape();
                    DrawingPriority = 1;
                    IsReceiveDamage = false;
                    break;
                default:
                    collisionShape = new asd.RectangleShape();
                    break;
            }
        }

        public void SetSensor(string name, asd.Vector2DF position, float diameter = 3)
        {
            Sensor temp = new Sensor(position, diameter);
            sensors.Add(name, temp);
        }

        public Sensor GetSensorData(string name)
        {
            if (!sensors.ContainsKey(name)) return null;
            return sensors[name];
        }

        public void SetSound(string name, string path, bool isMultiplePlay = false)
        {
            sounds[name] = new Sound(path, isMultiplePlay, true);
        }

        public int PlaySound(string name)
        {
            if (!sounds.ContainsKey(name)) return -1;
            return sounds[name].Play();
        }

        public void SetChild(string name, string animationpath, string scriptPath)
        {
            MapObject temp = new MapObject(subQueue, mainQueue, scriptPath, refWorld);
            childMapObjectData.Add(name, temp);
        }

        public void CreateChild(string name, asd.Vector2DF position)
        {
            if (!childMapObjectData.ContainsKey(name)) return;
            MapObject temp = (MapObject)childMapObjectData[name].Clone();
            temp.Position = position;
            Layer.AddObject(temp);
        }

        public new object Clone()
        {
            MapObject clone = new MapObject();
            clone.sensors = new Dictionary<string, Sensor>(sensors);
            clone.childMapObjectData = new Dictionary<string, MapObject>(childMapObjectData);
            clone.sounds = new Dictionary<string, Sound>(sounds);
            clone.Effects = new Dictionary<string, Effect>(Effects);
            clone.refWorld = refWorld;
            clone.Update = Update;
            clone.State = State;
            clone.Clone(this);
            clone.SetType(MapObjectType);
            try
            {
                clone.collisionShape.DrawingArea = new asd.RectF(new asd.Vector2DF(), clone.AnimationPart.First().Value.Textures.First().Size.To2DF());
            }
            catch (Exception e)
            {
                ErrorIO.AddError(e);
            }
            clone.CenterPosition = clone.collisionShape.DrawingArea.Size / 2;
            if (MapObjectType == MapObjectType.Active)
            {
                clone.CollisionShape.GroupIndex = CollisionShape.GroupIndex;
                clone.CollisionShape.MaskBits = CollisionShape.MaskBits;
                clone.CollisionShape.CategoryBits = CollisionShape.CategoryBits;
            }
            return clone;
        }

        public void LoadEffect(string animationGroup, string extension, int sheets, string name, int interval)
        {
            Effect effect = new Effect();
            effect.LoadEffect(animationGroup, extension, sheets, interval);
            Effects.Add(name, effect);
        }

        public void SetEffect(string name, asd.Vector2DF position)
        {
            if (!Effects.ContainsKey(name)) return;
            Effect effect = (Effect)Effects[name].Clone();
            effect.Position = Position + position;
            Layer.AddObject(effect);
        }

        public bool GetIsColligedWith(asd.Shape shape)
        {
            return collisionShape.GetIsCollidedWith(shape);
        }

        public Player Player { get; protected set; }

        public class Sensor
        {
            asd.Vector2DF position;
            asd.CircleShape circleShape;
            public bool IsColligedWithCollisions { get; set; }
            public bool IsColligedWithPlayer { get; set; }
            public bool IsColligedWithMapObjects { get; set; }

            public Sensor(asd.Vector2DF sensorPosition, float diameter)
            {
                circleShape = new asd.CircleShape();
                position = sensorPosition;
                circleShape.OuterDiameter = diameter;
                IsColligedWithCollisions = false;
                IsColligedWithMapObjects = false;
                IsColligedWithPlayer = false;
            }

            public void Update(MapObject mapObject)
            {
                circleShape.Position = mapObject.Position + position;
                IsColligedWithCollisions = mapObject.RefMainMapLayer2D.CollisionShapes.Any(obj => obj.GetIsCollidedWith(circleShape));
                IsColligedWithMapObjects = mapObject.RefMainMapLayer2D.MapObjects.Any(obj =>
                    {
                        if (obj.MapObjectType == MapObjectType.Passive) return false;
                        return obj.CollisionShape.GetIsCollidedWith(circleShape);
                    });
                IsColligedWithPlayer = mapObject.RefMainMapLayer2D.Player != null ? mapObject.RefMainMapLayer2D.Player.CollisionShape.GetIsCollidedWith(circleShape) : false;
            }
        }
    }

    public enum MapObjectType
    {
        Active,
        Passive,
    }
}
