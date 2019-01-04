using SatIO;
using PhysicAltseed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor
{
    public class NPCMapObject : MapObject, ICopyPasteObject
    {
        private string _motionPath;

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

        public float WalkSpeed { get; set; }

        public float DashSpeed { get; set; }

        public float JumpPower { get; set; }

        [TextOutput("ID")]
        public int ID { get; set; }

        [VectorInput("座標")]
        public new asd.Vector2DF Position { get => base.Position; set => base.Position = value; }

        /// <summary>
        /// スクリプトへのパス
        /// </summary>
        [FileInput("スクリプト", "Script File|*.csx|All File|*.*")]
        public new string ScriptPath
        {
            get => base.ScriptPath;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                base.ScriptPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// モーションへのパス
        /// </summary>
        [FileInput("モーション", "Motion File|*.mo|All File|*.*")]
        public string MotionPath
        {
            get => _motionPath;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _motionPath = value;
                try
                {
                    LoadMotion(BaseIO.Load<MotionIO>(_motionPath));
                    State = UprightLeftState;
                    CenterPosition = Texture.Size.To2DF() / 2;
                    CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition, Texture.Size.To2DF());
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
                OnPropertyChanged();
            }
        }

        public NPCMapObject()
        {

        }

        public NPCMapObject(NPCMapObjectIO mapObject)
        {
            ScriptPath = mapObject.ScriptPath;
            Position = mapObject.Position;
            MotionPath = mapObject.MotionPath;
            ID = mapObject.ID;
        }

        [Button("消去")]
        public new void OnClickRemove()
        {
            UndoRedoManager.ChangeObject2D(Layer, this, false);
            Layer.RemoveObject(this);
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

        public new ICopyPasteObject Copy()
        {
            UndoRedoManager.Enable = false;
            NPCMapObject copy = new NPCMapObject();
            copy.ScriptPath = ScriptPath;
            copy.Position = Position + new asd.Vector2DF(50, 50);
            copy.MotionPath = MotionPath;
            return copy;
        }

        public static explicit operator NPCMapObjectIO(NPCMapObject mapObject)
        {
            var result = new NPCMapObjectIO()
            {
                ScriptPath = mapObject.ScriptPath,
                MotionPath = mapObject.MotionPath,
                Position = mapObject.Position,
                ID = mapObject.ID,
            };
            return result;
        }
    }
}
