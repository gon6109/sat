using BaseComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SatIO
{
    [Serializable()]
    public class MotionIO
    {
        public MotionIO()
        {
            WalkLeftMotion = new AnimationIO();
            WalkRightMotion = new AnimationIO();
            DashLeftMotion = new AnimationIO();
            DashRightMotion = new AnimationIO();
            JumpLeftMotion = new AnimationIO();
            JumpRightMotion = new AnimationIO();
            UpLeftMotion = new AnimationIO();
            UpRightMotion = new AnimationIO();
            DownLeftMotion = new AnimationIO();
            DownRightMotion = new AnimationIO();
            UpperLeftMotion = new AnimationIO();
            UpperRightMotion = new AnimationIO();
            LowerLeftMotion = new AnimationIO();
            LowerRightMotion = new AnimationIO();
            DashUpperLeftMotion = new AnimationIO();
            DashUpperRightMotion = new AnimationIO();
            DashLowerLeftMotion = new AnimationIO();
            DashLowerRightMotion = new AnimationIO();
            UprightLeftMotion = new AnimationIO();
            UprightRightMotion = new AnimationIO();
            WalkSpeed = 100;
            DashSpeed = 200;
            JumpPower = 4700;
        }

        public AnimationIO WalkLeftMotion { get; set; }
        public AnimationIO WalkRightMotion { get; set; }
        public AnimationIO DashLeftMotion { get; set; }
        public AnimationIO DashRightMotion { get; set; }
        public AnimationIO JumpLeftMotion { get; set; }
        public AnimationIO JumpRightMotion { get; set; }
        public AnimationIO UpLeftMotion { get; set; }
        public AnimationIO UpRightMotion { get; set; }
        public AnimationIO DownLeftMotion { get; set; }
        public AnimationIO DownRightMotion { get; set; }
        public AnimationIO UpperLeftMotion { get; set; }
        public AnimationIO UpperRightMotion { get; set; }
        public AnimationIO LowerLeftMotion { get; set; }
        public AnimationIO LowerRightMotion { get; set; }
        public AnimationIO DashUpperLeftMotion { get; set; }
        public AnimationIO DashUpperRightMotion { get; set; }
        public AnimationIO DashLowerLeftMotion { get; set; }
        public AnimationIO DashLowerRightMotion { get; set; }
        public AnimationIO UprightLeftMotion { get; set; }
        public AnimationIO UprightRightMotion { get; set; }

        public float WalkSpeed { get; set; }
        public float DashSpeed { get; set; }
        public float JumpPower { get; set; }


        [Serializable()]
        public class AnimationIO
        {
            public string AnimationGroup { get; set; }
            public int Sheets { get; set; }
            public int Interval { get; set; }

            public AnimationIO(string animationGroup, int sheets, int interval)
            {
                AnimationGroup = animationGroup;
                Sheets = sheets;
                Interval = interval;
            }

            public AnimationIO()
            {
                AnimationGroup = "";
                Sheets = 1;
                Interval = 1;
            }
        }

        public void SaveMotionIO(string path)
        {
            using (FileStream motionFile = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(motionFile, this);
            }
        }

        public static MotionIO GetMotionIO(string path)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            return (MotionIO)serializer.Deserialize(IO.GetStream(path));
        }
    }
}
