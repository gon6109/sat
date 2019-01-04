using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BaseComponent;
using PhysicAltseed;

namespace SatCore.MotionEditor
{
    public class Character : MultiAnimationObject2D, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
                if (Texture != null) CenterPosition = Texture.Size.To2DF() / 2.0f;
            }
        }

        public PhysicalRectangleShape CollisionShape { set; get; }

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

        float walkSpeed;
        [NumberInput("歩行速度")]
        public int WalkSpeed
        {
            get => (int)walkSpeed;
            set
            {
                if (value > 0)
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    walkSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        float dashSpeed;
        [NumberInput("走行速度")]
        public int DashSpeed
        {
            get => (int)dashSpeed;
            set
            {
                if (value > 0)
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    dashSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        float jumpPower;
        private Motion _walkLeftMotion;
        private Motion _walkRightMotion;
        private Motion _dashLeftMotion;
        private Motion _dashRightMotion;
        private Motion _upLeftMotion;
        private Motion _upRightMotion;
        private Motion _downLeftMotion;
        private Motion _downRightMotion;
        private Motion _upperLeftMotion;
        private Motion _upperRightMotion;
        private Motion _lowerLeftMotion;
        private Motion _lowerRightMotion;
        private Motion _dashUpperLeftMotion;
        private Motion _dashUpperRightMotion;
        private Motion _dashLowerLeftMotion;
        private Motion _dashLowerRightMotion;
        private Motion _uprightLeftMotion;
        private Motion _uprightRightMotion;
        private Motion _jumpLeftMotion;
        private Motion _jumpRightMotion;

        [NumberInput("跳躍力")]
        public int JumpPower
        {
            get => (int)jumpPower;
            set
            {
                if (value > 0)
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    jumpPower = value;
                    OnPropertyChanged();
                }
            }
        }

        [VectorInput("当たり判定サイズ")]
        public asd.Vector2DF CollisionSize
        {
            get => CollisionShape.DrawingArea.Size;
            set
            {
                Position = new asd.Vector2DF(400, 400);
                if (AnimationPart.Count == 0 || AnimationPart.First().Value.Textures.Count == 0) CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition, value);
                else
                {
                    CenterPosition = AnimationPart.First().Value.Textures.First().Size.To2DF() / 2.0f;
                    CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition + new asd.Vector2DF((AnimationPart.First().Value.Textures.First().Size.X - value.X) / 2, AnimationPart.First().Value.Textures.First().Size.Y - value.Y), value);
                }
            }
        }

        [Group("左歩き")]
        public Motion WalkLeftMotion
        {
            get => _walkLeftMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value); _walkLeftMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("右歩き")]
        public Motion WalkRightMotion
        {
            get => _walkRightMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value); _walkRightMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("左走り")]
        public Motion DashLeftMotion
        {
            get => _dashLeftMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value); _dashLeftMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("右走り")]
        public Motion DashRightMotion
        {
            get => _dashRightMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value); _dashRightMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("左上昇")]
        public Motion UpLeftMotion
        {
            get => _upLeftMotion;
            set
            {
                _upLeftMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("右上昇")]
        public Motion UpRightMotion
        {
            get => _upRightMotion;
            set
            {
                _upRightMotion = value;
                OnPropertyChanged();
            }

        }

        [Group("左下降")]
        public Motion DownLeftMotion
        {
            get => _downLeftMotion;
            set
            {
                _downLeftMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("右下降")]
        public Motion DownRightMotion
        {
            get => _downRightMotion;
            set
            {
                _downRightMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("左上")]
        public Motion UpperLeftMotion
        {
            get => _upperLeftMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _upperLeftMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("右上")]
        public Motion UpperRightMotion
        {
            get => _upperRightMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _upperRightMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("左下")]
        public Motion LowerLeftMotion
        {
            get => _lowerLeftMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _lowerLeftMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("右下")]
        public Motion LowerRightMotion
        {
            get => _lowerRightMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _lowerRightMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("左上走り")]
        public Motion DashUpperLeftMotion
        {
            get => _dashUpperLeftMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _dashUpperLeftMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("右上走り")]
        public Motion DashUpperRightMotion
        {
            get => _dashUpperRightMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _dashUpperRightMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("左下走り")]
        public Motion DashLowerLeftMotion
        {
            get => _dashLowerLeftMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _dashLowerLeftMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("右下走り")]
        public Motion DashLowerRightMotion
        {
            get => _dashLowerRightMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _dashLowerRightMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("左直立")]
        public Motion UprightLeftMotion
        {
            get => _uprightLeftMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _uprightLeftMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("右直立")]
        public Motion UprightRightMotion
        {
            get => _uprightRightMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _uprightRightMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("左ジャンプ")]
        public Motion JumpLeftMotion
        {
            get => _jumpLeftMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _jumpLeftMotion = value;
                OnPropertyChanged();
            }
        }

        [Group("右ジャンプ")]
        public Motion JumpRightMotion
        {
            get => _jumpRightMotion;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _jumpRightMotion = value;
                OnPropertyChanged();
            }
        }

        public virtual bool IsColligedWithGround { get; private set; }

        public asd.RectangleShape GroundShape { get; set; }

        public Character(string characterDataPath, PhysicalWorld world)
        {
            LoadMotion(SatIO.MotionIO.Load<SatIO.MotionIO>(characterDataPath));
            base.Position = new asd.Vector2DF();
            GroundShape = new asd.RectangleShape();
            CollisionShape = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, world);
        }

        public Character(PhysicalWorld world)
        {
            LoadMotion(new SatIO.MotionIO());
            base.Position = new asd.Vector2DF();
            GroundShape = new asd.RectangleShape();
            CollisionShape = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, world);
        }

        protected override void OnAdded()
        {
            State = UprightRightState;
            if (Texture != null) CenterPosition = Texture.Size.To2DF() / 2.0f;
            CollisionShape.Density = 2.5f;
            CollisionShape.Restitution = 0.0f;
            CollisionShape.Friction = 0.0f;
            DrawingPriority = 2;

            GroundShape.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);
            Layer.AddObject(new asd.GeometryObject2D() { Shape = GroundShape, Color = new asd.Color(255, 0, 0) });
            base.OnAdded();
        }

        protected override void OnUpdate()
        {
            base.Position = CollisionShape.CenterPosition + CollisionShape.DrawingArea.Position;
            if (Math.Abs(CollisionShape.Angle) > 1.0f) CollisionShape.AngularVelocity = -CollisionShape.Angle * 30.0f;
            GroundShape.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);

            IsColligedWithGround = Layer.Objects.Any(obj => obj is asd.GeometryObject2D && ((asd.GeometryObject2D)obj).Shape != GroundShape && ((asd.GeometryObject2D)obj).Shape.GetIsCollidedWith(GroundShape));

            InputPlayer();
            Action();
            base.OnUpdate();
        }

        protected virtual void InputPlayer()
        {
            if (!State.Contains("jump") && IsColligedWithGround)
            {
                if (Input.GetInputState(Inputs.Up) == 1)
                {
                    State = State.Contains("_l") ? JumpLeftState : JumpRightState;
                    return;
                }
                if (Input.GetInputState(Inputs.Left) > 0 && Input.GetInputState(Inputs.B) < 1) State = WalkLeftState;
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.B) < 1) State = WalkRightState;
                if (Input.GetInputState(Inputs.Left) > 0 && Input.GetInputState(Inputs.B) > 0) State = DashLeftState;
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.B) > 0) State = DashRightState;
                if ((Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.Left) > 0)
                    || (Input.GetInputState(Inputs.Right) < 1 && Input.GetInputState(Inputs.Left) < 1))
                {
                    State = State.Contains("_l") ? UprightLeftState : UprightRightState;
                }
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
            }
        }

        protected virtual void Action()
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

        public void LoadMotion(SatIO.MotionIO motion)
        {
            WalkSpeed = (int)motion.WalkSpeed;
            DashSpeed = (int)motion.DashSpeed;
            JumpPower = (int)motion.JumpPower;

            WalkLeftMotion = Motion.GetMotion(motion.WalkLeftMotion, WalkLeftState, this);
            WalkRightMotion = Motion.GetMotion(motion.WalkRightMotion, WalkRightState, this);
            DashLeftMotion = Motion.GetMotion(motion.DashLeftMotion, DashLeftState, this);
            DashRightMotion = Motion.GetMotion(motion.DashRightMotion, DashRightState, this);
            JumpLeftMotion = Motion.GetMotion(motion.JumpLeftMotion, JumpLeftState, this);
            JumpRightMotion = Motion.GetMotion(motion.JumpRightMotion, JumpRightState, this);
            UpLeftMotion = Motion.GetMotion(motion.UpLeftMotion, UpLeftState, this);
            UpRightMotion = Motion.GetMotion(motion.UpRightMotion, UpRightState, this);
            DownLeftMotion = Motion.GetMotion(motion.DownLeftMotion, DownLeftState, this);
            DownRightMotion = Motion.GetMotion(motion.DownRightMotion, DownRightState, this);
            UpperLeftMotion = Motion.GetMotion(motion.UpperLeftMotion, UpperLeftState, this);
            UpperRightMotion = Motion.GetMotion(motion.UpperRightMotion, UpperRightState, this);
            LowerLeftMotion = Motion.GetMotion(motion.LowerLeftMotion, LowerLeftState, this);
            LowerRightMotion = Motion.GetMotion(motion.LowerRightMotion, LowerRightState, this);
            DashUpperLeftMotion = Motion.GetMotion(motion.DashUpperLeftMotion, DashUpperLeftState, this);
            DashUpperRightMotion = Motion.GetMotion(motion.DashUpperRightMotion, DashUpperRightState, this);
            DashLowerLeftMotion = Motion.GetMotion(motion.DashLowerLeftMotion, DashLowerLeftState, this);
            DashLowerRightMotion = Motion.GetMotion(motion.DashLowerRightMotion, DashLowerRightState, this);
            UprightLeftMotion = Motion.GetMotion(motion.UprightLeftMotion, UprightLeftState, this);
            UprightRightMotion = Motion.GetMotion(motion.UprightRightMotion, UprightRightState, this);
        }

        public void UpdateMotion(SatIO.MotionIO motion)
        {
            WalkSpeed = (int)motion.WalkSpeed;
            DashSpeed = (int)motion.DashSpeed;
            JumpPower = (int)motion.JumpPower;

            WalkLeftMotion.UpdateData(motion.WalkLeftMotion, WalkLeftState, this);
            WalkRightMotion.UpdateData(motion.WalkRightMotion, WalkRightState, this);
            DashLeftMotion.UpdateData(motion.DashLeftMotion, DashLeftState, this);
            DashRightMotion.UpdateData(motion.DashRightMotion, DashRightState, this);
            JumpLeftMotion.UpdateData(motion.JumpLeftMotion, JumpLeftState, this);
            JumpRightMotion.UpdateData(motion.JumpRightMotion, JumpRightState, this);
            UpLeftMotion.UpdateData(motion.UpLeftMotion, UpLeftState, this);
            UpRightMotion.UpdateData(motion.UpRightMotion, UpRightState, this);
            DownLeftMotion.UpdateData(motion.DownLeftMotion, DownLeftState, this);
            DownRightMotion.UpdateData(motion.DownRightMotion, DownRightState, this);
            UpperLeftMotion.UpdateData(motion.UpperLeftMotion, UpperLeftState, this);
            UpperRightMotion.UpdateData(motion.UpperRightMotion, UpperRightState, this);
            LowerLeftMotion.UpdateData(motion.LowerLeftMotion, LowerLeftState, this);
            LowerRightMotion.UpdateData(motion.LowerRightMotion, LowerRightState, this);
            DashUpperLeftMotion.UpdateData(motion.DashUpperLeftMotion, DashUpperLeftState, this);
            DashUpperRightMotion.UpdateData(motion.DashUpperRightMotion, DashUpperRightState, this);
            DashLowerLeftMotion.UpdateData(motion.DashLowerLeftMotion, DashLowerLeftState, this);
            DashLowerRightMotion.UpdateData(motion.DashLowerRightMotion, DashLowerRightState, this);
            UprightLeftMotion.UpdateData(motion.UprightLeftMotion, UprightLeftState, this);
            UprightRightMotion.UpdateData(motion.UprightRightMotion, UprightRightState, this);
        }

        public SatIO.MotionIO ToMotionIO()
        {
            SatIO.MotionIO motionIO = new SatIO.MotionIO();
            motionIO.WalkSpeed = WalkSpeed;
            motionIO.DashSpeed = DashSpeed;
            motionIO.JumpPower = JumpPower;
            motionIO.WalkLeftMotion = (SatIO.MotionIO.AnimationIO)WalkLeftMotion;
            motionIO.WalkRightMotion = (SatIO.MotionIO.AnimationIO)WalkRightMotion;
            motionIO.DashLeftMotion = (SatIO.MotionIO.AnimationIO)DashLeftMotion;
            motionIO.DashRightMotion = (SatIO.MotionIO.AnimationIO)DashRightMotion;
            motionIO.JumpLeftMotion = (SatIO.MotionIO.AnimationIO)JumpLeftMotion;
            motionIO.JumpRightMotion = (SatIO.MotionIO.AnimationIO)JumpRightMotion;
            motionIO.UpLeftMotion = (SatIO.MotionIO.AnimationIO)UpLeftMotion;
            motionIO.UpRightMotion = (SatIO.MotionIO.AnimationIO)UpRightMotion;
            motionIO.DownLeftMotion = (SatIO.MotionIO.AnimationIO)DownLeftMotion;
            motionIO.DownRightMotion = (SatIO.MotionIO.AnimationIO)DownRightMotion;
            motionIO.UpperLeftMotion = (SatIO.MotionIO.AnimationIO)UpperLeftMotion;
            motionIO.UpperRightMotion = (SatIO.MotionIO.AnimationIO)UpperRightMotion;
            motionIO.LowerLeftMotion = (SatIO.MotionIO.AnimationIO)LowerLeftMotion;
            motionIO.LowerRightMotion = (SatIO.MotionIO.AnimationIO)LowerRightMotion;
            motionIO.DashUpperLeftMotion = (SatIO.MotionIO.AnimationIO)DashUpperLeftMotion;
            motionIO.DashUpperRightMotion = (SatIO.MotionIO.AnimationIO)DashUpperRightMotion;
            motionIO.DashLowerLeftMotion = (SatIO.MotionIO.AnimationIO)DashLowerLeftMotion;
            motionIO.DashLowerRightMotion = (SatIO.MotionIO.AnimationIO)DashLowerRightMotion;
            motionIO.UprightLeftMotion = (SatIO.MotionIO.AnimationIO)UprightLeftMotion;
            motionIO.UprightRightMotion = (SatIO.MotionIO.AnimationIO)UprightRightMotion;
            return motionIO;
        }

        public class Motion : INotifyPropertyChanged, IListInput
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public string Name { get; private set; }
            Character refCharacter;
            private string _animationGroup;
            private int _sheets;
            private int _interval;

            public Motion(string animationGroup, int sheets, string name, int interval, Character character)
            {
                Name = name;
                refCharacter = character;
                _animationGroup = animationGroup;
                _sheets = sheets;
                _interval = interval;
                refCharacter.AddAnimationPart(AnimationGroup, "png", Sheets, Name, Interval);
            }

            [FileInput("ファイル", "AnimationFile|*0.png")]
            public string AnimationGroup
            {
                get => _animationGroup;
                set
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    _animationGroup = value.Replace("0.png", "");
                    refCharacter.AddAnimationPart(AnimationGroup, "png", Sheets, Name, Interval);
                    OnPropertyChanged();
                }
            }

            [NumberInput("枚数")]
            public int Sheets
            {
                get => _sheets;
                set
                {
                    if (value > 0)
                    {
                        UndoRedoManager.ChangeProperty(this, value);
                        _sheets = value;
                        refCharacter.AddAnimationPart(AnimationGroup, "png", Sheets, Name, Interval);
                        OnPropertyChanged();
                    }
                }
            }

            [NumberInput("フレーム数/コマ")]
            public int Interval
            {
                get => _interval;
                set
                {
                    if (value > 0)
                    {
                        UndoRedoManager.ChangeProperty(this, value);
                        _interval = value;
                        refCharacter.AddAnimationPart(AnimationGroup, "png", Sheets, Name, Interval);
                        OnPropertyChanged();
                    }
                }
            }

            public void UpdateData(SatIO.MotionIO.AnimationIO animationIO, string name, Character character)
            {
                AnimationGroup = animationIO.AnimationGroup;
                Sheets = animationIO.Sheets;
                Interval = animationIO.Interval;
                Name = name;
                refCharacter = character;
            }

            public static explicit operator SatIO.MotionIO.AnimationIO(Motion motion)
            {
                return new SatIO.MotionIO.AnimationIO(motion.AnimationGroup, motion.Sheets, motion.Interval);
            }

            public static Motion GetMotion(SatIO.MotionIO.AnimationIO animationIO, string name, Character character)
            {
                return new Motion(animationIO.AnimationGroup, animationIO.Sheets, name, animationIO.Interval, character);
            }
        }
    }
}
