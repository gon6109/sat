using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using PhysicAltseed;
using System.Runtime.Serialization.Formatters.Binary;
using BaseComponent;

namespace SatPlayer
{
    //TODO: 書き直す
    /// <summary>
    /// プレイヤー
    /// </summary>
    public class Player : MultiAnimationObject2D, IEffectManeger, IMotion, IDamageControler
    {
        public static int MaxHP = 100;

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

        public new string State
        {
            get => base.State;
            set
            {
                base.State = value;
                CenterPosition = Texture != null ? Texture.Size.To2DF() / 2.0f : AnimationPart.First(obj => obj.Value.Textures.Count > 0).Value.Textures.First().Size.To2DF();
            }
        }

        public int PlayerGroup { get; set; }

        public PhysicalRectangleShape CollisionShape { set; get; }

        public Dictionary<string, Effect> Effects { get; private set; }

        public string WalkLeftState => "walk_l";
        public string WalkRightState => "walk_r";
        public string DashLeftState => "dash_l";
        public string DashRightState => "dash_r";
        public string UpLeftState => "up_l";
        public string UpRightState => "up_r";
        public string DownLeftState => "down_l";
        public string DownRightState => "down_r";
        public string UpperLeftState => "upper_l";
        public string UpperRightState => "upper_r";
        public string LowerLeftState => "lower_l";
        public string LowerRightState => "lower_r";
        public string DashUpperLeftState => "dash_upper_l";
        public string DashUpperRightState => "dash_upper_r";
        public string DashLowerLeftState => "dash_lower_l";
        public string DashLowerRightState => "dash_lower_r";
        public string UprightLeftState => "upright_l";
        public string UprightRightState => "upright_r";
        public string JumpLeftState => "jump_l";
        public string JumpRightState => "jump_r";

        public string WalkLeftEffect => "walk_l";
        public string WalkRightEffect => "walk_r";
        public string DashLeftEffect => "dash_l";
        public string DashRightEffect => "dash_r";
        public string JumpLeftEffect => "jump_l";
        public string JumpRightEffect => "jump_r";
        public string HitLeftEffect => "hit_l";
        public string HitRightEffect => "hit_r";
        public string ChargeLeftEffect => "charge_l";
        public string ChargeRightEffect => "charge_r";
        Dictionary<string, asd.Vector2DF> effectPositions;

        int actionCounter;
        public int ActionCounter
        {
            get
            {
                if (BaseComponent.Input.GetInputState(Inputs.A) == -1) return actionCounter;
                return 0;
            }
        }

        public bool IsColligedWithGround { get; private set; }

        public bool IsEvent { get; set; }

        float walkSpeed;
        public float WalkSpeed
        {
            get => walkSpeed;
            set
            {
                if (value > 0) walkSpeed = value;
            }
        }

        float dashSpeed;
        public float DashSpeed
        {
            get => dashSpeed;
            set
            {
                if (value > 0) dashSpeed = value;
            }
        }

        float jumpPower;
        public float JumpPower
        {
            get => jumpPower;
            set
            {
                if (value > 0) jumpPower = value;
            }
        }

        public int HP
        {
            get => hP;
            set
            {
                if (hP > value) SetEffect(State.Contains("_l") ? HitLeftEffect : HitRightEffect,
                    effectPositions[State.Contains("_l") ? HitLeftEffect : HitRightEffect]);
                hP = value;
            }
        }
        public bool IsReceiveDamage { get; set; }

        public Queue<DamageRect> DamageRequests { get; private set; }

        public DamageRect.OwnerType OwnerType => DamageRect.OwnerType.Player;

        asd.Shape IDamageControler.CollisionShape => CollisionShape;

        public Queue<DirectDamage> DirectDamageRequests { get; private set; }

        public Queue<Dictionary<Inputs, bool>> MoveCommands { get; private set; }

        public int ID => -1;

        public string Name { get; private set; }

        public bool IsUseName => true;

        asd.RectangleShape groundShape;
        private int hP;

        public Player(string playerDataPath, int playerGroup = 0)
        {
            CameraGroup = 1;
            base.Position = new asd.Vector2DF();
            Effects = new Dictionary<string, Effect>();
            groundShape = new asd.RectangleShape();
            actionCounter = 0;
            IsColligedWithGround = false;
            DamageRequests = new Queue<DamageRect>();
            DirectDamageRequests = new Queue<DirectDamage>();
            effectPositions = new Dictionary<string, asd.Vector2DF>();
            MoveCommands = new Queue<Dictionary<Inputs, bool>>();
            PlayerGroup = playerGroup;
            LoadPlayerData(playerDataPath);
        }

        protected override void OnAdded()
        {
            State = UprightRightState;
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

            if (!IsEvent) InputPlayer();
            else if (MoveCommands.Count != 0) InputPlayer(MoveCommands.Dequeue());
            else InputPlayer(new Dictionary<Inputs, bool>());
            Action();
            base.OnUpdate();
        }

        void InputPlayer()
        {
            if (!State.Contains("jump") && IsColligedWithGround)//TODO: コンボ中は受け付けない
            {
                if (Input.GetInputState(Inputs.Left) > 0 && Input.GetInputState(Inputs.B) < 1) State = WalkLeftState;
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.B) < 1) State = WalkRightState;
                if (Input.GetInputState(Inputs.Left) > 0 && Input.GetInputState(Inputs.B) > 0) State = DashLeftState;
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.B) > 0) State = DashRightState;
                if (Input.GetInputState(Inputs.Up) == 1)
                {
                    State = State.Contains("_l") ? JumpLeftState : JumpRightState;
                    SetEffect(State.Contains("_l") ? JumpLeftEffect : JumpRightEffect, effectPositions[State.Contains("_l") ? JumpLeftEffect : JumpRightEffect]);
                    return;
                }
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.Left) > 0) State = State.Contains("_l") ? UprightLeftState : UprightRightState;
                if (Input.GetInputState(Inputs.Right) < 1 && Input.GetInputState(Inputs.Left) < 1) State = State.Contains("_l") ? UprightLeftState : UprightRightState;
                if (Input.GetInputState(Inputs.A) > 20) State = "charge" + (State.Contains("_l") ? "_l" : "_r");
            }
            else
            {
                if (Input.GetInputState(Inputs.Left) > 0 && Input.GetInputState(Inputs.B) < 1) State = CollisionShape.Velocity.Y < 0 ? UpperLeftState : LowerLeftState;
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.B) < 1) State = CollisionShape.Velocity.Y < 0 ? UpperRightState : LowerRightState;
                if (Input.GetInputState(Inputs.Left) > 0 && Input.GetInputState(Inputs.B) > 0) State = CollisionShape.Velocity.Y < 0 ? DashUpperLeftState : DashLowerLeftState;
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.B) > 0) State = CollisionShape.Velocity.Y < 0 ? DashUpperRightState : DashLowerRightState;
                if ((Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.Left) > 0)
                    || (Input.GetInputState(Inputs.Right) < 1 && Input.GetInputState(Inputs.Left) < 1))
                {
                    if (CollisionShape.Velocity.Y < 0) State = State.Contains("_l") ? UpLeftState : UpRightState;
                    else State = State.Contains("_l") ? DownLeftState : DownRightState;
                }
                if (Input.GetInputState(Inputs.Up) == -1 && State.Contains("up") && !State.Contains("upright"))
                {
                    CollisionShape.Velocity = new asd.Vector2DF(0, CollisionShape.Velocity.Y / 2);
                }
            }

            if (Input.GetInputState(Inputs.A) != -1) actionCounter = Input.GetInputState(Inputs.A);
        }

        void InputPlayer(Dictionary<Inputs, bool> moveCommand)
        {

            if (!State.Contains("jump") && IsColligedWithGround)
            {
                if (GetInputState(moveCommand, Inputs.Up))
                {
                    State = State.Contains("_l") ? JumpLeftState : JumpRightState;
                    return;
                }
                if (GetInputState(moveCommand, Inputs.Left) && !GetInputState(moveCommand, Inputs.B)) State = WalkLeftState;
                if (GetInputState(moveCommand, Inputs.Right) && !GetInputState(moveCommand, Inputs.B)) State = WalkRightState;
                if (GetInputState(moveCommand, Inputs.Left) && GetInputState(moveCommand, Inputs.B)) State = DashLeftState;
                if (GetInputState(moveCommand, Inputs.Right) && GetInputState(moveCommand, Inputs.B)) State = DashRightState;
                if ((GetInputState(moveCommand, Inputs.Right) && GetInputState(moveCommand, Inputs.Left))
                    || (!GetInputState(moveCommand, Inputs.Right) && !GetInputState(moveCommand, Inputs.Left)))
                {
                    State = State.Contains("_l") ? UprightLeftState : UprightRightState;
                }
            }
            else
            {
                if (GetInputState(moveCommand, Inputs.Left) && !GetInputState(moveCommand, Inputs.B)) State = CollisionShape.Velocity.Y < 0 ? UpperLeftState : LowerLeftState;
                if (GetInputState(moveCommand, Inputs.Right) && !GetInputState(moveCommand, Inputs.B)) State = CollisionShape.Velocity.Y < 0 ? UpperRightState : LowerRightState;
                if (GetInputState(moveCommand, Inputs.Left) && GetInputState(moveCommand, Inputs.B)) State = CollisionShape.Velocity.Y < 0 ? DashUpperLeftState : DashLowerLeftState;
                if (GetInputState(moveCommand, Inputs.Right) && GetInputState(moveCommand, Inputs.B)) State = CollisionShape.Velocity.Y < 0 ? DashUpperRightState : DashLowerRightState;
                if ((GetInputState(moveCommand, Inputs.Right) && GetInputState(moveCommand, Inputs.Left))
                    || (!GetInputState(moveCommand, Inputs.Right) && !GetInputState(moveCommand, Inputs.Left)))
                {
                    if (CollisionShape.Velocity.Y < 0) State = State.Contains("_l") ? UpLeftState : UpRightState;
                    else State = State.Contains("_l") ? DownLeftState : DownRightState;
                }
                if (Input.GetInputState(Inputs.Up) == -1 && State.Contains("up") && !State.Contains("upright"))
                {
                    CollisionShape.Velocity = new asd.Vector2DF(0, CollisionShape.Velocity.Y / 2);
                }
            }
        }

        void Action()
        {
            if (ActionCounter > 20)
            {
                State = State.Contains("_l") ? UprightLeftState : UprightRightState;
                State = "heavy" + (State.Contains("_l") ? "_l" : "_r");
                IsOneLoop = true;
                //TODO: タメ攻撃
                //SetEffect(State.Contains("_l") ?  : UprightRightState);
                CollisionShape.Velocity = new asd.Vector2DF(0, CollisionShape.Velocity.Y);
            }
            else if (ActionCounter > 0)
            {
                State = "light" + (State.Contains("_l") ? "_l" : "_r");
                //TODO: コンボ
                CollisionShape.Velocity = new asd.Vector2DF(0, 0);
            }
            else if (State == WalkLeftState || State == UpperLeftState || State == LowerLeftState) CollisionShape.Velocity = new asd.Vector2DF(-WalkSpeed, CollisionShape.Velocity.Y);
            else if (State == WalkRightState || State == UpperRightState || State == LowerRightState) CollisionShape.Velocity = new asd.Vector2DF(WalkSpeed, CollisionShape.Velocity.Y);
            else if (State == DashLeftState || State == DashUpperLeftState || State == DashLowerLeftState) CollisionShape.Velocity = new asd.Vector2DF(-DashSpeed, CollisionShape.Velocity.Y);
            else if (State == DashRightState || State == DashUpperRightState || State == DashLowerRightState) CollisionShape.Velocity = new asd.Vector2DF(DashSpeed, CollisionShape.Velocity.Y);
            else if (State.Contains("jump"))
            {
                CollisionShape.SetImpulse(new asd.Vector2DF(0.0f, -JumpPower), CollisionShape.CenterPosition);
            }
            else CollisionShape.Velocity = new asd.Vector2DF(0, CollisionShape.Velocity.Y);

            if (State == WalkLeftState) SetEffect(WalkLeftEffect, effectPositions[WalkLeftEffect]);
            if (State == WalkRightState) SetEffect(WalkRightEffect, effectPositions[WalkRightEffect]);
            if (State == DashLeftState) SetEffect(DashLeftEffect, effectPositions[DashLeftEffect]);
            if (State == DashRightState) SetEffect(DashRightEffect, effectPositions[DashRightEffect]);
        }

        public void LoadEffect(string animationGroup, string extension, int sheets, string name, int interval)
        {
            Effect effect = new Effect();
            effect.LoadEffect(animationGroup, extension, sheets, interval);
            Effects[name] = effect;
        }

        public void SetEffect(string name, asd.Vector2DF position)
        {
            if (!Effects.ContainsKey(name)) return;
            Effect effect = (Effect)Effects[name].Clone();
            effect.Position = Position + position;
            Layer.AddObject(effect);
        }

        public void LoadPlayerData(string playerDataPath)
        {
            LoadPlayerData(SatIO.BaseIO.Load<SatIO.PlayerIO>(playerDataPath));
        }

        public void LoadPlayerData(SatIO.PlayerIO playerData)
        {
            LoadMotion(playerData);

            LoadEffect(playerData.WalkLeftEffect.AnimationGroup, "png", playerData.WalkLeftEffect.Sheets, WalkLeftEffect, playerData.WalkLeftEffect.Interval);
            effectPositions[WalkLeftEffect] = playerData.WalkLeftEffect.Position;
            LoadEffect(playerData.WalkRightEffect.AnimationGroup, "png", playerData.WalkRightEffect.Sheets, WalkRightEffect, playerData.WalkRightEffect.Interval);
            effectPositions[WalkRightEffect] = playerData.WalkLeftEffect.Position;
            LoadEffect(playerData.DashLeftEffect.AnimationGroup, "png", playerData.DashLeftEffect.Sheets, DashLeftEffect, playerData.DashLeftEffect.Interval);
            effectPositions[DashLeftEffect] = playerData.DashLeftEffect.Position;
            LoadEffect(playerData.DashRightEffect.AnimationGroup, "png", playerData.DashRightEffect.Sheets, DashRightEffect, playerData.DashRightEffect.Interval);
            effectPositions[DashRightEffect] = playerData.DashRightEffect.Position;
            LoadEffect(playerData.JumpLeftEffect.AnimationGroup, "png", playerData.JumpLeftEffect.Sheets, JumpLeftEffect, playerData.JumpLeftEffect.Interval);
            effectPositions[JumpLeftEffect] = playerData.JumpLeftEffect.Position;
            LoadEffect(playerData.JumpRightEffect.AnimationGroup, "png", playerData.JumpRightEffect.Sheets, JumpRightEffect, playerData.JumpRightEffect.Interval);
            effectPositions[JumpRightEffect] = playerData.JumpLeftEffect.Position;
            LoadEffect(playerData.HitLeftEffect.AnimationGroup, "png", playerData.HitLeftEffect.Sheets, HitLeftEffect, playerData.HitLeftEffect.Interval);
            effectPositions[HitLeftEffect] = playerData.HitLeftEffect.Position;
            LoadEffect(playerData.HitRightEffect.AnimationGroup, "png", playerData.HitRightEffect.Sheets, HitRightEffect, playerData.HitRightEffect.Interval);
            effectPositions[HitRightEffect] = playerData.HitRightEffect.Position;
            LoadEffect(playerData.ChargeLeftEffect.AnimationGroup, "png", playerData.ChargeLeftEffect.Sheets, ChargeLeftEffect, playerData.ChargeLeftEffect.Interval);
            effectPositions[ChargeLeftEffect] = playerData.ChargeLeftEffect.Position;
            LoadEffect(playerData.ChargeRightEffect.AnimationGroup, "png", playerData.ChargeRightEffect.Sheets, ChargeRightEffect, playerData.ChargeRightEffect.Interval);
            effectPositions[ChargeRightEffect] = playerData.ChargeRightEffect.Position;

            Name = playerData.Name;
        }

        public void LoadMotion(SatIO.MotionIO motion)
        {
            WalkSpeed = motion.WalkSpeed;
            DashSpeed = motion.DashSpeed;
            JumpPower = motion.JumpPower;

            AddAnimationPart(motion.WalkLeftMotion.AnimationGroup, "png", motion.WalkLeftMotion.Sheets, WalkLeftState, motion.WalkLeftMotion.Interval);
            AddAnimationPart(motion.WalkRightMotion.AnimationGroup, "png", motion.WalkRightMotion.Sheets, WalkRightState, motion.WalkRightMotion.Interval);
            AddAnimationPart(motion.DashLeftMotion.AnimationGroup, "png", motion.DashLeftMotion.Sheets, DashLeftState, motion.DashLeftMotion.Interval);
            AddAnimationPart(motion.DashRightMotion.AnimationGroup, "png", motion.DashRightMotion.Sheets, DashRightState, motion.DashRightMotion.Interval);
            AddAnimationPart(motion.JumpLeftMotion.AnimationGroup, "png", motion.JumpLeftMotion.Sheets, JumpLeftState, motion.JumpLeftMotion.Interval);
            AddAnimationPart(motion.JumpRightMotion.AnimationGroup, "png", motion.JumpRightMotion.Sheets, JumpRightState, motion.JumpRightMotion.Interval);
            AddAnimationPart(motion.UpLeftMotion.AnimationGroup, "png", motion.UpLeftMotion.Sheets, UpLeftState, motion.UpLeftMotion.Interval);
            AddAnimationPart(motion.UpRightMotion.AnimationGroup, "png", motion.UpRightMotion.Sheets, UpRightState, motion.UpRightMotion.Interval);
            AddAnimationPart(motion.DownLeftMotion.AnimationGroup, "png", motion.DownLeftMotion.Sheets, DownLeftState, motion.DownLeftMotion.Interval);
            AddAnimationPart(motion.DownRightMotion.AnimationGroup, "png", motion.DownRightMotion.Sheets, DownRightState, motion.DownRightMotion.Interval);
            AddAnimationPart(motion.UpperLeftMotion.AnimationGroup, "png", motion.UpperLeftMotion.Sheets, UpperLeftState, motion.UpperLeftMotion.Interval);
            AddAnimationPart(motion.UpperRightMotion.AnimationGroup, "png", motion.UpperRightMotion.Sheets, UpperRightState, motion.UpperRightMotion.Interval);
            AddAnimationPart(motion.LowerLeftMotion.AnimationGroup, "png", motion.LowerLeftMotion.Sheets, LowerLeftState, motion.LowerLeftMotion.Interval);
            AddAnimationPart(motion.LowerRightMotion.AnimationGroup, "png", motion.LowerRightMotion.Sheets, LowerRightState, motion.LowerRightMotion.Interval);
            AddAnimationPart(motion.DashUpperLeftMotion.AnimationGroup, "png", motion.DashUpperLeftMotion.Sheets, DashUpperLeftState, motion.DashUpperLeftMotion.Interval);
            AddAnimationPart(motion.DashUpperRightMotion.AnimationGroup, "png", motion.DashUpperRightMotion.Sheets, DashUpperRightState, motion.DashUpperRightMotion.Interval);
            AddAnimationPart(motion.DashLowerLeftMotion.AnimationGroup, "png", motion.DashLowerLeftMotion.Sheets, DashLowerLeftState, motion.DashLowerLeftMotion.Interval);
            AddAnimationPart(motion.DashLowerRightMotion.AnimationGroup, "png", motion.DashLowerRightMotion.Sheets, DashLowerRightState, motion.DashLowerRightMotion.Interval);
            AddAnimationPart(motion.UprightLeftMotion.AnimationGroup, "png", motion.UprightLeftMotion.Sheets, UprightLeftState, motion.UprightLeftMotion.Interval);
            AddAnimationPart(motion.UprightRightMotion.AnimationGroup, "png", motion.UprightRightMotion.Sheets, UprightRightState, motion.UprightRightMotion.Interval);
        }

        bool GetInputState(Dictionary<Inputs, bool> moveCommand, Inputs inputs)
        {
            return moveCommand.ContainsKey(inputs) ? moveCommand[inputs] : false;
        }
    }
}
