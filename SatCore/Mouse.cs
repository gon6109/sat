using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    /// <summary>
    /// マウス入力モデル
    /// </summary>
    public static class Mouse
    {
        public static asd.Vector2DF Position { get; set; } = new asd.Vector2DF();

        static bool preLeft, currentLeft;

        /// <summary>
        /// 左ボタンの状態を設定
        /// </summary>
        public static bool IsLeftButton
        {
            set
            {
                preLeft = currentLeft;
                currentLeft = value;
            }
        }

        /// <summary>
        /// 左ボタンの状態を取得
        /// </summary>
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

        /// <summary>
        /// 右ボタンの状態を設定
        /// </summary>
        public static bool IsRightButton
        {
            set
            {
                preRight = currentRight;
                currentRight = value;
            }
        }

        /// <summary>
        /// 右ボタンの状態を取得
        /// </summary>
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

        /// <summary>
        /// 中央ボタンの状態を設定
        /// </summary>
        public static bool IsMiddleButton
        {
            set
            {
                preMiddle = currentMiddle;
                currentMiddle = value;
            }
        }

        /// <summary>
        /// 中央ボタンの状態を取得
        /// </summary>
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

        /// <summary>
        /// ホイール回転状態を設定・取得
        /// </summary>
        public static int MouseWheel { get; set; } = 0;

    }
}
