using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using PhysicAltseed;
using System.Runtime.Serialization.Formatters.Binary;
using BaseComponent;
using SatScript.Player;
using AltseedScript.Common;
using SatScript.Collision;
using System.Threading.Tasks;

namespace SatPlayer.Game.Object
{
    /// <summary>
    /// プレイヤー
    /// </summary>
    public class Player : MultiAnimationObject2D, IEffectManeger, IPlayer, IDamageControler, IActor
    {
        public static int MaxHP = 100;

        /// <summary>
        /// 現在座標
        /// </summary>
        public new asd.Vector2DF Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                if (CollisionShape != null) CollisionShape.DrawingArea = new asd.RectF(value - CollisionShape.CenterPosition, CollisionShape.DrawingArea.Size);
            }
        }

        /// <summary>
        /// 現在座標
        /// </summary>
        Vector IPlayer.Position { get => Position.ToScriptVector(); set => Position = value.ToAsdVector(); }

        /// <summary>
        /// アニメーション状態
        /// </summary>
        public new string State
        {
            get => base.State;
            set
            {
                base.State = value;
                CenterPosition = Texture != null ? 
                    Texture.Size.To2DF() / 2.0f : 
                    AnimationPart.FirstOrDefault(obj => obj.Value.Textures.Count > 0).Value?.
                    Textures.FirstOrDefault()?.Size.To2DF() ?? default;
            }
        }

        /// <summary>
        /// プレイヤーグループ
        /// </summary>
        public int PlayerGroup { get; set; }

        public PhysicalRectangleShape CollisionShape { protected set; get; }

        public Dictionary<string, object> Effects { get; private set; }

        /// <summary>
        /// 地面と接しているか
        /// </summary>
        public bool IsCollidedWithGround { get; protected set; }

        /// <summary>
        /// イベント時か
        /// </summary>
        public bool IsEvent { get; set; }

        /// <summary>
        /// HP
        /// </summary>
        public int HP
        {
            get => hP;
            set
            {
                if (hP > value)
                {
                    //TODO: ダメージ
                }
                hP = value;
            }
        }

        /// <summary>
        /// ダメージを受けるか
        /// </summary>
        public bool IsReceiveDamage { get; set; }

        public Queue<DamageRect> DamageRequests { get; private set; }

        public DamageRect.OwnerType OwnerType => DamageRect.OwnerType.Player;

        asd.Shape IDamageControler.CollisionShape => CollisionShape;

        public Queue<DirectDamage> DirectDamageRequests { get; private set; }

        public Queue<Dictionary<BaseComponent.Inputs, bool>> MoveCommands { get; private set; }
        Dictionary<BaseComponent.Inputs, int> inputState;

        public int ID => -1;

        public string Name { get; set; }

        public string Path { get; protected set; }

        /// <summary>
        /// 衝突情報
        /// </summary>
        public ICollision Collision { get; set; }

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
        /// OnUpdate時に呼び出されイベント
        /// </summary>
        public virtual event Action<IPlayer> Update = delegate { };

        PhysicalShape IActor.CollisionShape => CollisionShape;

        public asd.RectangleShape GroundCollision { get; }
        Color IPlayer.Color { get => Color.ToScriptColor(); set => Color = value.ToAsdColor(); }
        protected List<Task> LoadTextureTasks { get; } = new List<Task>();

        private int hP;

        public Player()
        {
            GroundCollision = new asd.RectangleShape();
            base.Position = new asd.Vector2DF();
            Effects = new Dictionary<string, object>();
            IsCollidedWithGround = false;
            DamageRequests = new Queue<DamageRect>();
            DirectDamageRequests = new Queue<DirectDamage>();
            MoveCommands = new Queue<Dictionary<BaseComponent.Inputs, bool>>();
            inputState = new Dictionary<BaseComponent.Inputs, int>();
            foreach (BaseComponent.Inputs item in Enum.GetValues(typeof(BaseComponent.Inputs)))
            {
                inputState[item] = 0;
            }
        }

        protected override void OnAdded()
        {
            if (Layer is MapLayer map)
                CollisionShape = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, map.PhysicalWorld);

            CollisionShape.Density = 2.5f;
            CollisionShape.Restitution = 0.0f;
            CollisionShape.Friction = 0.0f;
            CollisionShape.GroupIndex = -1;
            DrawingPriority = 2;

            SetCollision();

            base.OnAdded();
        }

        protected override void OnRemoved()
        {
            CollisionShape?.Dispose();
            CollisionShape = null;
            base.OnRemoved();
        }

        protected override void OnDispose()
        {
            CollisionShape?.Dispose();
            base.OnDispose();
        }

        protected override void OnUpdate()
        {
            base.Position = CollisionShape.CenterPosition + CollisionShape.DrawingArea.Position;
            if (Math.Abs(CollisionShape.Angle) > 1.0f) CollisionShape.AngularVelocity = -CollisionShape.Angle * 30.0f;
            GroundCollision.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);

            if (Layer is MapLayer layer)
            {
                IsCollidedWithGround = layer.Obstacles.Any(obj => obj.GetIsCollidedWith(GroundCollision));
            }

            if (IsEvent && MoveCommands.Count > 0)
            {
                var currentCommand = MoveCommands.Dequeue();
                foreach (BaseComponent.Inputs item in Enum.GetValues(typeof(BaseComponent.Inputs)))
                {
                    if (!currentCommand.ContainsKey(item)) inputState[item] = 0;
                    else if (currentCommand[item] && inputState[item] > -1) inputState[item]++;
                    else if (currentCommand[item] && inputState[item] == -1) inputState[item] = 1;
                    else if (!currentCommand[item] && inputState[item] > 0) inputState[item] = -1;
                    else inputState[item] = 0;
                }
            }

            try
            {
                Update(this);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Update = obj => { };
            }
            base.OnUpdate();
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
            Effects[name] = effect;
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
        /// 入力状態を得る
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public int GetInputState(AltseedScript.Common.Inputs inputs)
        {
            if (IsEvent) return inputState[(BaseComponent.Inputs)inputs];
            else return AltseedScript.Common.Input.GetInputState(inputs);
        }

        /// <summary>
        /// 力を加える
        /// </summary>
        /// <param name="direct">力の向き・強さ</param>
        /// <param name="position">力を加える場所の相対座標</param>
        public void SetForce(Vector direct, Vector position)
        {
            CollisionShape.SetForce(direct.ToAsdVector(), position.ToAsdVector() + CenterPosition);
        }

        /// <summary>
        /// 衝撃を加える
        /// </summary>
        /// <param name="direct">力の向き・強さ</param>
        /// <param name="position">力を加える芭蕉の相対座標</param>
        public void SetImpulse(Vector direct, Vector position)
        {
            CollisionShape.SetImpulse(direct.ToAsdVector(), position.ToAsdVector() + CenterPosition);
        }

        void IActor.OnUpdate()
        {
            OnUpdate();
        }

        protected void SetCollision()
        {
            if (Texture == null)
                Texture = AnimationPart.FirstOrDefault().Value?.Textures.FirstOrDefault();
            CenterPosition = Texture?.Size.To2DF() / 2.0f ?? new asd.Vector2DF();
            CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition + new asd.Vector2DF(5, 0), Texture?.Size.To2DF() ?? new asd.Vector2DF() - new asd.Vector2DF(10, 0));
            GroundCollision.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);
        }

        void IPlayer.AddAnimationPart(string animationGroup, string extension, int sheets, string partName, int interval)
        {
            LoadTextureTasks.Add(AddAnimationPartAsync(animationGroup, extension, sheets, partName, interval));
        }

        public static async Task<Player> CreatePlayerAsync(string playerDataPath, int playerGroup = 0)
        {
            try
            {
                var player = new Player();
                player.PlayerGroup = playerGroup;
                player.Path = playerDataPath;
                var stream = await IO.GetStreamAsync(playerDataPath);
                using (stream)
                {
                    var script = ScriptOption.ScriptOptions["Player"].CreateScript<object>(Encoding.UTF8.GetString(stream.ToArray()));
                    await script.RunAsync(player);
                    await Task.WhenAll(player.LoadTextureTasks);
                    player.LoadTextureTasks.Clear();
                }
                return player;
            }
            catch
            {
                throw;
            }
        }
    }
}
