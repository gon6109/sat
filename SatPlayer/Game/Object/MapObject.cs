using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
using System;
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
        public Dictionary<string, Effect> Effects { get; protected set; }

        /// <summary>
        /// OnUpdate時に呼び出されるイベント
        /// </summary>
        public virtual event Action<IMapObject> Update = delegate { };

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
        public Dictionary<string, ISensor> Sensors => sensors.ToDictionary(obj => obj.Key, obj => (ISensor)obj.Value);

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

        protected Dictionary<string, Sensor> sensors;
        protected Dictionary<string, MapObject> childMapObjectData;
        private int hP;
        private MapObjectType _mapObjectType;
        private short _collisionGroup;
        private ushort _collisionCategory;
        private ushort _collisionMask;

        public MapObject()
        {
            Init();
        }

        void Init()
        {
            CameraGroup = 1;
            HP = 100;
            DamageRequests = new Queue<DamageRect>();
            sensors = new Dictionary<string, Sensor>();
            Effects = new Dictionary<string, Effect>();
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
            if (collision is PhysicalRectangleShape shape)
            {
                base.Position = shape.CenterPosition + shape.DrawingArea.Position;
                if (Math.Abs(shape.Angle) > 1.0f && !IsAllowRotation) shape.AngularVelocity = -shape.Angle * 30.0f;
                if (IsAllowRotation) Angle = shape.Angle;
            }

            try
            {
                Update(this);
            }
            catch (Exception e)
            {
                ErrorIO.AddError(e);
                Dispose();
            }

            base.OnUpdate();
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
            clone.sensors = new Dictionary<string, Sensor>(sensors);
            clone.childMapObjectData = new Dictionary<string, MapObject>(childMapObjectData);
            clone.Effects = new Dictionary<string, Effect>(Effects);
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
                ErrorIO.AddError(e);
            }
            clone.CenterPosition = clone.collision.DrawingArea.Size / 2;
            clone.CollisionGroup = CollisionGroup;
            clone.CollisionMask = CollisionMask;
            clone.CollisionCategory = CollisionCategory;
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
        {
            if (collision is PhysicalRectangleShape shape)
                shape.SetForce(direct.ToAsdVector(), position.ToAsdVector());
        }

        /// <summary>
        /// 衝撃を加える
        /// </summary>
        /// <param name="direct">力の向き・強さ</param>
        /// <param name="position">力を加える芭蕉の相対座標</param>
        public void SetImpulse(Vector direct, Vector position)
        {
            if (collision is PhysicalRectangleShape shape)
                shape.SetImpulse(direct.ToAsdVector(), position.ToAsdVector());
        }

        /// <summary>
        /// コリジョンを設定する
        /// </summary>
        void SetCollisionShape()
        {
            if (Layer is MapLayer map)
            {
                if (collision != null)
                    collision.Dispose();
                switch (MapObjectType)
                {
                    case MapObjectType.Active:
                        collision = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, map.PhysicalWorld);
                        if (collision is PhysicalRectangleShape physicalRectangleShape)
                        {
                            physicalRectangleShape.DrawingArea = new asd.RectF(Position - physicalRectangleShape.DrawingArea.Size / 2, AnimationPart.FirstOrDefault().Value?.Textures.FirstOrDefault()?.Size.To2DF() ?? default);
                            CenterPosition = physicalRectangleShape.DrawingArea.Size / 2;
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

        public static async Task<MapObject> CreateMapObjectAsync(MapObjectIO mapObjectIO)
        {
            var mapObject = new MapObject();
            if (mapObjectIO.ScriptPath != "")
            {
                try
                {
                    var stream = await IO.GetStreamAsync(mapObjectIO.ScriptPath);
                    using (stream)
                    {
                        var script = ScriptOption.ScriptOptions["MapObject"]?.CreateScript<object>(stream.ToString());
                        await Task.Run(() => script.Compile());
                        await script.RunAsync(mapObject);
                    }
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
            }
            mapObject.Position = mapObjectIO.Position;
            return mapObject;
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

            public Sensor(asd.Vector2DF sensorPosition, float diameter)
            {
                circleShape = new asd.CircleShape();
                position = sensorPosition;
                circleShape.OuterDiameter = diameter;
            }

            public bool GetIsCollidedWith(asd.Shape shape)
                => circleShape.GetIsCollidedWith(shape);

            public bool GetIsCollidedWith(PhysicalShape shape)
                => shape.GetIsCollidedWith(circleShape);
        }
    }
}
