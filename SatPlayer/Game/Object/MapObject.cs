using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SatIO;
using BaseComponent;
using System.Collections.Concurrent;
using SatScript.MapObject;
using AltseedScript.Common;
using SatScript.Collision;
using System.Threading.Tasks;

namespace SatPlayer.Game.Object
{
    /// <summary>
    /// マップオブジェクト
    /// </summary>
    public class MapObject : MultiAnimationObject2D, IEffectManeger, ICloneable, IMapObject, IDamageControler
    {
        /// <summary>
        /// オブジェクト認識用タグ
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 座標
        /// </summary>
        public new asd.Vector2DF Position
        {
            get => base.Position;

            set
            {
                base.Position = value;
                if (collision != null)
                {
                    if (collision is PhysicalRectangleShape physicalRectangleShape)
                        physicalRectangleShape.DrawingArea = new asd.RectF(Position - physicalRectangleShape.DrawingArea.Size / 2, physicalRectangleShape.DrawingArea.Size);
                    else
                        collision.DrawingArea = new asd.RectF(value - collision.DrawingArea.Size / 2, collision.DrawingArea.Size);
                }
            }
        }

        /// <summary>
        /// 座標
        /// </summary>
        Vector IMapObject.Position
        {
            get => Position.ToScriptVector();
            set => Position = value.ToAsdVector();
        }

        /// <summary>
        /// アニメーション状態
        /// </summary>
        public new string State
        {
            get => base.State;
            set
            {
                base.State = value;
                IsOneLoop = false;
            }
        }

        /// <summary>
        /// マップオブジェクトのタイプ
        /// </summary>
        public MapObjectType MapObjectType
        {
            get => _mapObjectType;
            set
            {
                _mapObjectType = value;
                switch (value)
                {
                    case MapObjectType.Active:
                        DrawingPriority = 2;
                        IsReceiveDamage = true;
                        break;
                    case MapObjectType.Passive:
                        DrawingPriority = 1;
                        IsReceiveDamage = false;
                        break;
                    default:
                        break;
                }
                SetCollisionShape();
            }
        }

        protected asd.RectangleShape collision;
        public asd.RectangleShape CollisionShape => collision;

        /// <summary>
        /// エフェクト一覧
        /// </summary>
        public Dictionary<string, object> Effects { get; protected set; }

        /// <summary>
        /// OnUpdate時に呼び出されるイベント
        /// </summary>
        public event Action<IMapObject> Update = delegate { };

        /// <summary>
        /// HP
        /// </summary>
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

        /// <summary>
        /// ダメージを受けるか
        /// </summary>
        public bool IsReceiveDamage { get; set; }

        /// <summary>
        /// ダメージ領域発生要請キュー
        /// </summary>
        public Queue<DamageRect> DamageRequests { get; private set; }

        /// <summary>
        /// 陣営
        /// </summary>
        public DamageRect.OwnerType OwnerType { get; set; }

        asd.Shape IDamageControler.CollisionShape => collision;

        public Queue<DirectDamage> DirectDamageRequests { get; private set; }

        /// <summary>
        /// 衝突情報
        /// </summary>
        public ICollision Collision { get; set; }

        /// <summary>
        /// 速度
        /// </summary>
        public Vector Velocity
        {
            get
            {
                if (collision is PhysicalRectangleShape shape)
                    return shape.Velocity.ToScriptVector();
                else
                    return default;
            }
            set
            {
                if (collision is PhysicalRectangleShape shape)
                    shape.Velocity = value.ToAsdVector();
            }
        }

        /// <summary>
        /// 回転を許可するか
        /// </summary>
        public bool IsAllowRotation { get; set; }

        /// <summary>
        /// センサーを設定・取得する
        /// </summary>
        public IReadOnlyDictionary<string, ISensor> Sensors => sensors.ToDictionary(obj => obj.Key, obj => (ISensor)obj.Value);

        Color IMapObject.Color { get => Color.ToScriptColor(); set => Color = value.ToAsdColor(); }

        public short CollisionGroup
        {
            get => _collisionGroup;
            set
            {
                _collisionGroup = value;
                if (collision is PhysicalRectangleShape shape)
                    shape.GroupIndex = value;
            }
        }

        public ushort CollisionCategory
        {
            get => _collisionCategory;
            set
            {
                _collisionCategory = value;
                if (collision is PhysicalRectangleShape shape)
                    shape.CategoryBits = value;
            }
        }

        public ushort CollisionMask
        {
            get => _collisionMask;
            set
            {
                _collisionMask = value;
                if (collision is PhysicalRectangleShape shape)
                    shape.MaskBits = value;
            }
        }

        protected List<Task> LoadTextureTasks { get; } = new List<Task>();

        Dictionary<string, Sensor> sensors;
        Dictionary<string, MapObject> childMapObjectData;
        private int hP;
        private MapObjectType _mapObjectType;
        private short _collisionGroup = 0;
        private ushort _collisionCategory = 0x0001;
        private ushort _collisionMask = 0xffff;

        public MapObject()
        {
            Init();
        }

        void Init()
        {
            HP = 100;
            DamageRequests = new Queue<DamageRect>();
            sensors = new Dictionary<string, Sensor>();
            Effects = new Dictionary<string, object>();
            childMapObjectData = new Dictionary<string, MapObject>();
            collision = new asd.RectangleShape();
            DirectDamageRequests = new Queue<DirectDamage>();
            MapObjectType = MapObjectType.Passive;
        }

        protected override void OnAdded()
        {
            SetCollisionShape();
            base.OnAdded();
        }

        protected override void OnDispose()
        {
            Update = (obj) => { };
            if (collision is PhysicalRectangleShape shape)
                shape.Dispose();
            base.OnDispose();
        }

        protected override void OnUpdate()
        {
            UpdatePhysic();

            try
            {
                Update(this);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Dispose();
            }

            base.OnUpdate();
        }

        protected void UpdatePhysic()
        {
            if (collision is PhysicalRectangleShape shape)
            {
                base.Position = shape.CenterPosition + shape.DrawingArea.Position;
                if (Math.Abs(shape.Angle) > 1.0f && !IsAllowRotation) shape.AngularVelocity = -shape.Angle * 30.0f;
                if (IsAllowRotation) Angle = shape.Angle;
            }
        }

        /// <summary>
        /// 子MapObjectを設定する
        /// </summary>
        /// <param name="name">Object名</param>
        /// <param name="scriptPath">スクリプトパス</param>
        public void SetChild(string name, string scriptPath)
        {
            var task = MapObject.CreateMapObjectAsync(new MapObjectIO()
            {
                ScriptPath = scriptPath
            });
            task.Wait();
            MapObject temp = task.Result;
            childMapObjectData.Add(name, temp);
        }

        /// <summary>
        /// 子MapObjectを配置する
        /// </summary>
        /// <param name="name">Object名</param>
        /// <param name="position">スクリプトパス</param>
        public void CreateChild(string name, asd.Vector2DF position)
        {
            if (!childMapObjectData.ContainsKey(name)) return;
            MapObject temp = (MapObject)childMapObjectData[name].Clone();
            temp.Position = position;
            Layer.AddObject(temp);
        }

        /// <summary>
        /// 子MapObjectを配置する
        /// </summary>
        /// <param name="name">Object名</param>
        /// <param name="position">スクリプトパス</param>
        public void CreateChild(string name, Vector position)
            => CreateChild(name, position.ToAsdVector());

        public new object Clone()
        {
            MapObject clone = new MapObject();
            CloneImp(clone);
            return clone;
        }

        protected void CloneImp(MapObject clone, bool isPreview = false)
        {
            clone.sensors = CopySensors(clone, isPreview);
            clone.childMapObjectData = new Dictionary<string, MapObject>(childMapObjectData);
            clone.Effects = new Dictionary<string, object>(Effects);
            clone.Update = Update;
            clone.State = State;
            clone.Tag = Tag;
            clone.Copy(this);
            clone.MapObjectType = MapObjectType;
            clone.IsAllowRotation = IsAllowRotation;
            try
            {
                clone.collision.DrawingArea = new asd.RectF(new asd.Vector2DF(), clone.AnimationPart.First().Value.Textures.First().Size.To2DF());
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            clone.CenterPosition = clone.collision.DrawingArea.Size / 2;
            clone.CollisionGroup = CollisionGroup;
            clone.CollisionMask = CollisionMask;
            clone.CollisionCategory = CollisionCategory;
        }

        /// <summary>
        /// アニメーションエフェクトをロードする
        /// </summary>
        /// <param name="animationGroup">ファイル名</param>
        /// <param name="extension">拡張子</param>
        /// <param name="sheets">枚数</param>
        /// <param name="name">エフェクト名</param>
        /// <param name="interval">1コマ当たりのフレーム数</param>
        public void LoadEffect(string animationGroup, string extension, int sheets, string name, int interval)
        {
            Effect effect = new Effect();
            effect.LoadEffect(animationGroup, extension, sheets, interval);
            Effects.Add(name, effect);
        }

        /// <summary>
        /// Effekseerエフェクトをロードする
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public void LoadEffect(string path, string name)
        {
            asd.Effect effect = asd.Engine.Graphics.CreateEffect(path);
            if (effect == null)
            {
                Logger.Error(path + " not found.");
                return;
            }
            Effects.Add(name, effect);
        }

        /// <summary>
        /// エフェクトを配置する
        /// </summary>
        /// <param name="name">エフェクト名</param>
        /// <param name="position">座標</param>
        public void SetEffect(string name, asd.Vector2DF position)
        {
            if (!Effects.ContainsKey(name)) return;
            switch (Effects[name])
            {
                case Effect effect:
                    var newEffect = (Effect)effect.Clone();
                    effect.Position = Position + position;
                    Layer.AddObject(effect);
                    break;
                case asd.Effect asdEffect:
                    var effectObject = new EffekseerEffectObject2D();
                    effectObject.Effect = asdEffect;
                    effectObject.Position = Position + position;
                    Layer.AddObject(effectObject);
                    break;
                default:
                    Logger.Error("Undefined Effect Type.");
                    break;
            }
        }

        /// <summary>
        /// エフェクトを配置する
        /// </summary>
        /// <param name="name">エフェクト名</param>
        /// <param name="position">座標</param>
        public void SetEffect(string name, Vector position)
            => SetEffect(name, position.ToAsdVector());

        /// <summary>
        /// 力を加える
        /// </summary>
        /// <param name="direct">力の向き・強さ</param>
        /// 
        /// <param name="position">力を加える場所の相対座標</param>
        public void SetForce(Vector direct, Vector position)
        {
            if (collision is PhysicalRectangleShape shape)
                shape.SetForce(direct.ToAsdVector(), position.ToAsdVector() + CenterPosition);
        }

        /// <summary>
        /// 衝撃を加える
        /// </summary>
        /// <param name="direct">力の向き・強さ</param>
        /// <param name="position">力を加える芭蕉の相対座標</param>
        public void SetImpulse(Vector direct, Vector position)
        {
            if (collision is PhysicalRectangleShape shape)
                shape.SetImpulse(direct.ToAsdVector(), position.ToAsdVector() + CenterPosition);
        }

        /// <summary>
        /// コリジョンを設定する
        /// </summary>
        void SetCollisionShape()
        {
            if (Layer is MapLayer map)
            {
                if (collision is PhysicalShape shape)
                    shape.Dispose();
                switch (MapObjectType)
                {
                    case MapObjectType.Active:
                        collision = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, map.PhysicalWorld);
                        if (collision is PhysicalRectangleShape physicalRectangleShape)
                        {
                            physicalRectangleShape.DrawingArea = new asd.RectF(Position - physicalRectangleShape.DrawingArea.Size / 2, AnimationPart.FirstOrDefault().Value?.Textures.FirstOrDefault()?.Size.To2DF() ?? default);
                            CenterPosition = physicalRectangleShape.DrawingArea.Size / 2;
                            physicalRectangleShape.GroupIndex = CollisionGroup;
                            physicalRectangleShape.CategoryBits = CollisionCategory;
                            physicalRectangleShape.MaskBits = CollisionMask;
                        }
                        break;
                    case MapObjectType.Passive:
                        collision = new asd.RectangleShape();
                        break;
                    default:
                        collision = new asd.RectangleShape();
                        break;
                }
            }
        }

        public void SetSensor(string name, Vector position, float diameter = 3)
            =>
            sensors.Add(name, new Sensor(this, position.ToAsdVector(), diameter));

        void IMapObject.AddAnimationPart(string animationGroup, string extension, int sheets, string partName, int interval)
        {
            LoadTextureTasks.Add(AddAnimationPartAsync(animationGroup, extension, sheets, partName, interval));
        }

        public static async Task<MapObject> CreateMapObjectAsync(MapObjectIO mapObjectIO)
        {
            var mapObject = new MapObject();
            if (mapObjectIO.ScriptPath != "")
            {
                try
                {
                    using (var stream = await IO.GetStreamAsync(mapObjectIO.ScriptPath))
                    {
                        var script = ScriptOption.ScriptOptions["MapObject"]?.CreateScript<object>(Encoding.UTF8.GetString(stream.ToArray()));
                        await Task.Run(() => script.Compile());
                        await script.RunAsync(mapObject);
                        await Task.WhenAll(mapObject.LoadTextureTasks);
                        mapObject.LoadTextureTasks.Clear();
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            mapObject.Position = mapObjectIO.Position;
            return mapObject;
        }

        protected Dictionary<string, Sensor> CopySensors(MapObject to, bool isPreview = false)
        {
            var result = new Dictionary<string, Sensor>();
            foreach (var item in sensors)
            {
                result.Add(item.Key, new Sensor(to, item.Value.Position.ToAsdVector(), item.Value.Radius * 2, isPreview));
            }
            return result;
        }

        protected virtual void Reset()
        {
            sensors = new Dictionary<string, Sensor>();
            Effects = new Dictionary<string, object>();
            childMapObjectData = new Dictionary<string, MapObject>();
            Update = delegate { };
        }

        /// <summary>
        /// センサー
        /// </summary>
        public class Sensor : ISensor
        {
            asd.Vector2DF position;
            asd.CircleShape circleShape;

            /// <summary>
            /// 相対座標
            /// </summary>
            public Vector Position
            {
                get => position.ToScriptVector();
                set => position = value.ToAsdVector();
            }

            /// <summary>
            /// 半径
            /// </summary>
            public float Radius
            {
                get => circleShape.OuterDiameter / 2;
                set => circleShape.OuterDiameter = value * 2;
            }

            /// <summary>
            /// 衝突情報
            /// </summary>
            public ICollision Collision { get; set; }

            public MapObject Owner { get; private set; }

            public Sensor(MapObject owner, asd.Vector2DF sensorPosition, float diameter, bool isPreview = false)
            {
                circleShape = new asd.CircleShape();
                position = sensorPosition;
                circleShape.OuterDiameter = diameter;
                Owner = owner;
                if (isPreview)
                    Owner.AddChild(new asd.GeometryObject2D()
                    {
                        Shape = circleShape,
                        Color = new asd.Color(100, 0, 0)
                    },
                    (asd.ChildManagementMode)0b1111, asd.ChildTransformingMode.Nothing);
            }

            public bool GetIsCollidedWith(asd.Shape shape)
                => circleShape.GetIsCollidedWith(shape);

            public bool GetIsCollidedWith(PhysicalShape shape)
                => shape.GetIsCollidedWith(circleShape);

            public void Update()
            {
                if (Owner?.IsAlive ?? false)
                {
                    if (Owner.IsAllowRotation)
                    {
                        var pos = position;
                        pos.Degree += Owner.Angle;
                        circleShape.Position = Owner.Position + pos;
                    }
                    else
                        circleShape.Position = Owner.Position + position;
                }
            }
        }
    }
}
