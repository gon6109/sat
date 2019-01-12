﻿using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
using System;
using System.Collections.Generic;
using System.Linq;
using SatIO;
using BaseComponent;
using System.Collections.Concurrent;
using SatScript.MapObject;
using AlteseedScript.Common;
using SatScript.Collision;

namespace SatPlayer
{
    /// <summary>
    /// マップオブジェクト
    /// </summary>
    public class MapObject : MultiAnimationObject2D, IEffectManeger, ICloneable, IMapObject, IDamageControler
    {
        static ScriptOptions options = ScriptOptions.Default.WithImports("SatPlayer", "PhysicAltseed", "System", "System.Collections.Generic")
                                         .WithReferences(System.Reflection.Assembly.GetAssembly(typeof(IEnumerator<>))
                                                         , System.Reflection.Assembly.GetAssembly(typeof(MapObject))
                                                         , System.Reflection.Assembly.GetAssembly(typeof(MapObjectType))
                                                         , System.Reflection.Assembly.GetAssembly(typeof(asd.Vector2DF))
                                                         , System.Reflection.Assembly.GetAssembly(typeof(PhysicalRectangleShape)));

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
                if (MapObjectType == MapObjectType.Active) CollisionShape.DrawingArea = new asd.RectF(value - CollisionShape.CenterPosition, CollisionShape.DrawingArea.Size);
                else collisionShape.DrawingArea = new asd.RectF(value - collisionShape.DrawingArea.Size / 2, collisionShape.DrawingArea.Size);
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
                        collisionShape = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, refWorld);
                        DrawingPriority = 2;
                        IsReceiveDamage = true;
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
        }

        asd.RectangleShape collisionShape;
        public PhysicalRectangleShape CollisionShape { get => collisionShape as PhysicalRectangleShape; }

        /// <summary>
        /// エフェクト一覧
        /// </summary>
        public Dictionary<string, Effect> Effects { get; protected set; }

        /// <summary>
        /// マップレイヤーへの参照
        /// </summary>
        public MainMapLayer2D RefMainMapLayer2D => Layer as MainMapLayer2D;

        /// <summary>
        /// OnUpdate時に呼び出される関数のデリゲート
        /// </summary>
        public Action<IMapObject> Update { get; set; }

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

        asd.Shape IDamageControler.CollisionShape => collisionShape;

        public Queue<DirectDamage> DirectDamageRequests { get; private set; }

        /// <summary>
        /// 衝突情報
        /// </summary>
        public ICollision Collision => throw new NotImplementedException();

        /// <summary>
        /// 速度
        /// </summary>
        public Vector Velocity
        {
            get => CollisionShape?.Velocity.ToScriptVector() ?? new Vector();
            set
            {
                if (CollisionShape != null)
                    CollisionShape.Velocity = value.ToAsdVector();
            }
        }

        /// <summary>
        /// 回転を許可するか
        /// </summary>
        public bool IsAllowRotation { get; set; }

        /// <summary>
        /// センサーを設定・取得する
        /// </summary>
        public Dictionary<string, ISensor> Sensors => sensors.ToDictionary(obj => obj.Key, obj => (ISensor)obj.Value);

        protected Dictionary<string, Sensor> sensors;
        protected Dictionary<string, MapObject> childMapObjectData;
        protected PhysicalWorld refWorld;
        private int hP;

        BlockingCollection<Action> subQueue;
        BlockingCollection<Action> mainQueue;
        private MapObjectType _mapObjectType;

        public MapObject(BlockingCollection<Action> subThreadQueue, BlockingCollection<Action> mainThreadQueue, string scriptPath, PhysicalWorld world)
        {
            CameraGroup = 1;
            HP = 100;
            sensors = new Dictionary<string, Sensor>();
            childMapObjectData = new Dictionary<string, MapObject>();
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
            HP = 100;
            DamageRequests = new Queue<DamageRect>();
            sensors = new Dictionary<string, Sensor>();
            Effects = new Dictionary<string, Effect>();
            childMapObjectData = new Dictionary<string, MapObject>();
            Update = (obj) => { };
            collisionShape = new asd.RectangleShape();
            MapObjectType = MapObjectType.Passive;
        }

        protected override void OnAdded()
        {
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
            if (MapObjectType == MapObjectType.Active && !IsAllowRotation)
            {
                base.Position = CollisionShape.CenterPosition + CollisionShape.DrawingArea.Position;
                if (Math.Abs(CollisionShape.Angle) > 1.0f) CollisionShape.AngularVelocity = -CollisionShape.Angle * 30.0f;
            }

            foreach (var item in sensors) item.Value.Update(this);

            Update(this);

            base.OnUpdate();
        }

        /// <summary>
        /// 子MapObjectを設定する
        /// </summary>
        /// <param name="name">Object名</param>
        /// <param name="scriptPath">スクリプトパス</param>
        public void SetChild(string name, string scriptPath)
        {
            MapObject temp = new MapObject(subQueue, mainQueue, scriptPath, refWorld);
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
            clone.sensors = new Dictionary<string, Sensor>(sensors);
            clone.childMapObjectData = new Dictionary<string, MapObject>(childMapObjectData);
            clone.Effects = new Dictionary<string, Effect>(Effects);
            clone.refWorld = refWorld;
            clone.Update = Update;
            clone.State = State;
            clone.Clone(this);
            clone.MapObjectType = MapObjectType;
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

        /// <summary>
        /// エフェクトをロードする
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
        /// エフェクトを配置する
        /// </summary>
        /// <param name="name">エフェクト名</param>
        /// <param name="position">座標</param>
        public void SetEffect(string name, asd.Vector2DF position)
        {
            if (!Effects.ContainsKey(name)) return;
            Effect effect = (Effect)Effects[name].Clone();
            effect.Position = Position + position;
            Layer.AddObject(effect);
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
        /// <param name="position">力を加える場所の相対座標</param>
        public void SetForce(Vector direct, Vector position)
            => CollisionShape?.SetForce(direct.ToAsdVector(), position.ToAsdVector());

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

            public bool IsColligedWithCollisions { get; set; }
            public bool IsColligedWithPlayer { get; set; }
            public bool IsColligedWithMapObjects { get; set; }

            public ICollision Collision => throw new NotImplementedException();

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
}
