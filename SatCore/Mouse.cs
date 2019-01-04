using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    public static class Mouse
    {
        public static asd.Vector2DF Position { get; set; } = new asd.Vector2DF();

        static bool preLeft, currentLeft;
        public static bool IsLeftButton
        {
            set
            {
                preLeft = currentLeft;
                currentLeft = value;
            }
        }
        public static asd.ButtonState LeftButton
        {
            get
            {
                if (!preLeft && !currentLeft) return asd.ButtonState.Free;
                if (!preLeft && currentLeft) return asd.ButtonState.Push;
                if (preLeft && currentLeft) return asd.ButtonState.Hold;
                if (preLeft && !currentLeft) return asd.ButtonState.Release;
                return asd.ButtonState.Free;
            }
        }

        static bool preRight, currentRight;
        public static bool IsRightButton
        {
            set
            {
                preRight = currentRight;
                currentRight = value;
            }
        }
        public static asd.ButtonState RightButton
        {
            get
            {
                if (!preRight && !currentRight) return asd.ButtonState.Free;
                if (!preRight && currentRight) return asd.ButtonState.Push;
                if (preRight && currentRight) return asd.ButtonState.Hold;
                if (preRight && !currentRight) return asd.ButtonState.Release;
                return asd.ButtonState.Free;
            }
        }

        static bool preMiddle, currentMiddle;
        public static bool IsMiddleButton
        {
            set
            {
                preMiddle = currentMiddle;
                currentMiddle = value;
            }
        }
        public static asd.ButtonState MiddleButton
        {
            get
            {
                if (!preMiddle && !currentMiddle) return asd.ButtonState.Free;
                if (!preMiddle && currentMiddle) return asd.ButtonState.Push;
                if (preMiddle && currentMiddle) return asd.ButtonState.Hold;
                if (preMiddle && !currentMiddle) return asd.ButtonState.Release;
                return asd.ButtonState.Free;
            }
        }
        public static int MouseWheel { get; set; } = 0;

    }
}
