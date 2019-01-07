using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    [Obsolete]
    public interface IMotion
    {
        string State { get; set; }
        string WalkLeftState { get; }
        string WalkRightState { get; }
        string DashLeftState { get; }
        string DashRightState { get; }
        string JumpLeftState { get; }
        string JumpRightState { get; }
        string UpLeftState { get; }
        string UpRightState { get; }
        string DownLeftState { get; }
        string DownRightState { get; }
        string UpperLeftState { get; }
        string UpperRightState { get; }
        string LowerLeftState { get; }
        string LowerRightState { get; }
        string DashUpperLeftState { get; }
        string DashUpperRightState { get; }
        string DashLowerLeftState { get; }
        string DashLowerRightState { get; }
        string UprightLeftState { get; }
        string UprightRightState { get; }
        bool IsEvent { get; set; }
        float WalkSpeed { get; set; }
        float DashSpeed { get; set; }
        float JumpPower { get; set; }
        Queue<Dictionary<Inputs, bool>> MoveCommands { get; }
        int ID { get; }
        string Name { get; }
        bool IsUseName { get; }
        void LoadMotion(SatIO.MotionIO motion);
        asd.Vector2DF Position { get; set; }
    }

}
