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

namespace SatPlayer
{
    /// <summary>
    /// �v���C���[
    /// </summary>
    public class Player : MultiAnimationObject2D, IEffectManeger, IPlayer, IDamageControler, IActor
    {
        public static int MaxHP = 100;

        /// <summary>
        /// ���ݍ��W
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
        /// ���ݍ��W
        /// </summary>
        Vector IPlayer.Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// �A�j���[�V�������
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
        /// �v���C���[�O���[�v
        /// </summary>
        public int PlayerGroup { get; set; }

        public PhysicalRectangleShape CollisionShape { set; get; }

        public Dictionary<string, Effect> Effects { get; private set; }

        /// <summary>
        /// �n�ʂƐڂ��Ă��邩
        /// </summary>
        public bool IsCollidedWithGround { get; private set; }

        /// <summary>
        /// �C�x���g����
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
                    //TODO: �_���[�W
                }
                hP = value;
            }
        }

        /// <summary>
        /// �_���[�W���󂯂邩
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
        /// �Փˏ��
        /// </summary>
        public ICollision Collision { get; set; }

        /// <summary>
        /// ���x
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

        PhysicalShape IActor.CollisionShape => CollisionShape;

        public asd.RectangleShape GroundShape { get; }
        Color IPlayer.Color { get => Color.ToScriptColor(); set => Color = value.ToAsdColor(); }

        private int hP;

        public Player(string playerDataPath, int playerGroup = 0)
        {
            GroundShape = new asd.RectangleShape();
            PlayerGroup = playerGroup;
            Init();
            try
            {
                using (var stream = IO.GetStream(playerDataPath))
                {
                    var script = SatPlayer.ScriptOption.ScriptOptions["Player"].CreateScript<object>(stream.ToString());
                    var task = script.RunAsync(playerDataPath);
                    task.Wait();
                }
            }
            catch 
            {
                throw;
            }
        }

        protected Player(int playerGroup = 0)
        {
            GroundShape = new asd.RectangleShape();
            Init();
        }

        protected void Init()
        {
            CameraGroup = 1;
            base.Position = new asd.Vector2DF();
            Effects = new Dictionary<string, Effect>();
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
            CenterPosition = Texture?.Size.To2DF() / 2.0f ?? new asd.Vector2DF();

            CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition + new asd.Vector2DF(5, 0), Texture?.Size.To2DF() ?? new asd.Vector2DF() - new asd.Vector2DF(10, 0));
            CollisionShape.Density = 2.5f;
            CollisionShape.Restitution = 0.0f;
            CollisionShape.Friction = 0.0f;
            CollisionShape.GroupIndex = -1;
            DrawingPriority = 2;

            GroundShape.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);
            base.OnAdded();
        }

        protected override void OnUpdate()
        {
            base.Position = CollisionShape.CenterPosition + CollisionShape.DrawingArea.Position;
            if (Math.Abs(CollisionShape.Angle) > 1.0f) CollisionShape.AngularVelocity = -CollisionShape.Angle * 30.0f;
            GroundShape.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);

            if (Layer is MainMapLayer2D layer)
            {
                IsCollidedWithGround = layer.CollisionShapes.Any(obj => obj.GetIsCollidedWith(GroundShape));
            }

            if (IsEvent)
            {
                var currentCommand = MoveCommands.Dequeue();
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
        /// �G�t�F�N�g�����[�h����
        /// </summary>
        /// <param name="animationGroup">�t�@�C����</param>
        /// <param name="extension">�g���q</param>
        /// <param name="sheets">����</param>
        /// <param name="name">�G�t�F�N�g��</param>
        /// <param name="interval">1�R�}������̃t���[����</param>
        public void LoadEffect(string animationGroup, string extension, int sheets, string name, int interval)
        {
            Effect effect = new Effect();
            effect.LoadEffect(animationGroup, extension, sheets, interval);
            Effects[name] = effect;
        }

        /// <summary>
        /// �G�t�F�N�g��z�u����
        /// </summary>
        /// <param name="name">�G�t�F�N�g��</param>
        /// <param name="position">���W</param>
        public void SetEffect(string name, asd.Vector2DF position)
        {
            if (!Effects.ContainsKey(name)) return;
            Effect effect = (Effect)Effects[name].Clone();
            effect.Position = Position + position;
            Layer.AddObject(effect);
        }

        /// <summary>
        /// �G�t�F�N�g��z�u����
        /// </summary>
        /// <param name="name">�G�t�F�N�g��</param>
        /// <param name="position">���W</param>
        public void SetEffect(string name, Vector positon)
            => SetEffect(name, positon.ToAsdVector());

        /// <summary>
        /// ���͏�Ԃ𓾂�
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public int GetInputState(AltseedScript.Common.Inputs inputs)
        {
            if (IsEvent) return inputState[(BaseComponent.Inputs)inputs];
            else return AltseedScript.Common.Input.GetInputState(inputs);
        }

        void IActor.OnUpdate()
        {
            OnUpdate();
        }
    }
}
