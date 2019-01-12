using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using PhysicAltseed;
using System.Runtime.Serialization.Formatters.Binary;
using BaseComponent;
using SatScript.Player;
using AlteseedScript.Common;
using SatScript.Collision;

namespace SatPlayer
{
    /// <summary>
    /// プレイヤー
    /// </summary>
    public class Player : MultiAnimationObject2D, IEffectManeger, IPlayer, IDamageControler
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
        Vector IPlayer.Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// アニメーション状態
        /// </summary>
        public new string State
        {
            get => base.State;
            set
            {
                base.State = value;
                CenterPosition = Texture != null ? Texture.Size.To2DF() / 2.0f : AnimationPart.First(obj => obj.Value.Textures.Count > 0).Value.Textures.First().Size.To2DF();
            }
        }

        /// <summary>
        /// プレイヤーグループ
        /// </summary>
        public int PlayerGroup { get; set; }

        public PhysicalRectangleShape CollisionShape { set; get; }

        public Dictionary<string, Effect> Effects { get; private set; }

        /// <summary>
        /// 地面と接しているか
        /// </summary>
        public bool IsColligedWithGround { get; private set; }

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

        public bool IsUseName => true;

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

        public Action<IPlayer> Update { get; set; } = obj => { };

        asd.RectangleShape groundShape;
        private int hP;

        public Player(string playerDataPath, int playerGroup = 0)
        {
            CameraGroup = 1;
            base.Position = new asd.Vector2DF();
            Effects = new Dictionary<string, Effect>();
            groundShape = new asd.RectangleShape();
            IsColligedWithGround = false;
            DamageRequests = new Queue<DamageRect>();
            DirectDamageRequests = new Queue<DirectDamage>();
            MoveCommands = new Queue<Dictionary<BaseComponent.Inputs, bool>>();
            inputState = new Dictionary<BaseComponent.Inputs, int>();
            foreach (BaseComponent.Inputs item in Enum.GetValues(typeof(BaseComponent.Inputs)))
            {
                inputState[item] = 0;
            }
            PlayerGroup = playerGroup;
        }

        protected override void OnAdded()
        {
            CenterPosition = Texture.Size.To2DF() / 2.0f;

            CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition + new asd.Vector2DF(5, 0), Texture.Size.To2DF() - new asd.Vector2DF(10, 0));
            CollisionShape.Density = 2.5f;
            CollisionShape.Restitution = 0.0f;
            CollisionShape.Friction = 0.0f;
            CollisionShape.GroupIndex = -1;
            DrawingPriority = 2;

            groundShape.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);
            base.OnAdded();
        }

        protected override void OnUpdate()
        {
            base.Position = CollisionShape.CenterPosition + CollisionShape.DrawingArea.Position;
            if (Math.Abs(CollisionShape.Angle) > 1.0f) CollisionShape.AngularVelocity = -CollisionShape.Angle * 30.0f;
            groundShape.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);

            if (Layer is MainMapLayer2D)
            {
                IsColligedWithGround = ((MainMapLayer2D)Layer).CollisionShapes.Any(obj => obj.GetIsCollidedWith(groundShape));
            }

            var currentCommand = MoveCommands.Dequeue();
            if (IsEvent)
            {
                foreach (BaseComponent.Inputs item in Enum.GetValues(typeof(BaseComponent.Inputs)))
                {
                    if (currentCommand[item] && inputState[item] > -1) inputState[item]++;
                    else if (currentCommand[item] && inputState[item] == -1) inputState[item] = 1;
                    else if (!currentCommand[item] && inputState[item] > 0) inputState[item] = -1;
                    else inputState[item] = 0;
                }
            }

            Update(this);
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
        public void SetEffect(string name, Vector positon)
            => SetEffect(name, positon.ToAsdVector());

        /// <summary>
        /// 入力状態を得る
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public int GetInputState(AlteseedScript.Common.Inputs inputs)
        {
            if (IsEvent) return inputState[(BaseComponent.Inputs)inputs];
            else return AlteseedScript.Common.Input.GetInputState(inputs);
        }
    }
}
