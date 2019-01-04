using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseComponent;
using SatIO;
using SatPlayer.MapEvent;
using PhysicAltseed;

namespace SatPlayer
{
    public class NPCMapObject : MapObject, IMotion
    {
        private float walkSpeed;
        private float dashSpeed;
        private float jumpPower;

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

        public bool IsEvent { get; set; }

        public float WalkSpeed
        {
            get => walkSpeed;
            set
            {
                if (value > 0) walkSpeed = value;
            }
        }

        public float DashSpeed
        {
            get => dashSpeed;
            set
            {
                if (value > 0) dashSpeed = value;
            }
        }

        public float JumpPower
        {
            get => jumpPower;
            set
            {
                if (value > 0) jumpPower = value;
            }
        }

        public asd.RectangleShape GroundShape { get; set; }

        public virtual bool IsColligedWithGround { get; private set; }

        public Queue<Dictionary<Inputs, bool>> MoveCommands { get; private set; }
        public int ID { get; private set; }
        public string Name => "";
        public bool IsUseName => false;

        public NPCMapObject(BlockingCollection<Action> subThreadQueue, BlockingCollection<Action> mainThreadQueue, string scriptPath, PhysicalWorld world, string motionFilePath)
            : base(subThreadQueue, mainThreadQueue, scriptPath, world)
        {
            MoveCommands = new Queue<Dictionary<Inputs, bool>>();
            LoadMotion(MotionIO.Load<MotionIO>(motionFilePath));
            GroundShape = new asd.RectangleShape();
        }

        public void LoadMotion(MotionIO motion)
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

        protected override void OnUpdate()
        {
            if (!IsEvent)
            {
                base.OnUpdate();
                return;
            }

            base.Position = CollisionShape.CenterPosition + CollisionShape.DrawingArea.Position;
            if (Math.Abs(CollisionShape.Angle) > 1.0f) CollisionShape.AngularVelocity = -CollisionShape.Angle * 30.0f;
            GroundShape.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);

            if (Layer is MainMapLayer2D)
            {
                IsColligedWithGround = ((MainMapLayer2D)Layer).CollisionShapes.Any(obj => obj.GetIsCollidedWith(GroundShape));
            }

            if (MoveCommands.Count != 0) InputPlayer(MoveCommands.Dequeue());
            else InputPlayer(new Dictionary<Inputs, bool>());
            Action();
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
            }
        }

        void Action()
        {
            if (State == WalkLeftState || State == UpperLeftState || State == LowerLeftState) CollisionShape.Velocity = new asd.Vector2DF(-WalkSpeed, CollisionShape.Velocity.Y);
            else if (State == WalkRightState || State == UpperRightState || State == LowerRightState) CollisionShape.Velocity = new asd.Vector2DF(WalkSpeed, CollisionShape.Velocity.Y);
            else if (State == DashLeftState || State == DashUpperLeftState || State == DashLowerLeftState) CollisionShape.Velocity = new asd.Vector2DF(-DashSpeed, CollisionShape.Velocity.Y);
            else if (State == DashRightState || State == DashUpperRightState || State == DashLowerRightState) CollisionShape.Velocity = new asd.Vector2DF(DashSpeed, CollisionShape.Velocity.Y);
            else if (State.Contains("jump"))
            {
                CollisionShape.SetImpulse(new asd.Vector2DF(0.0f, -JumpPower), CollisionShape.CenterPosition);
            }
            else CollisionShape.Velocity = new asd.Vector2DF(0, CollisionShape.Velocity.Y);
        }

        bool GetInputState(Dictionary<Inputs, bool> moveCommand, Inputs inputs)
        {
            return moveCommand.ContainsKey(inputs) ? moveCommand[inputs] : false;
        }
    }
}
