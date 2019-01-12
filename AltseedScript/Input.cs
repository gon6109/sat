using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlteseedScript.Common
{
    /// <summary>
    /// 入力(BaseComponent.Inputのラッパー)
    /// </summary>
    public static class Input
    {
        public static int GetInputState(Inputs inputs)
        {
            return BaseComponent.Input.GetInputState((BaseComponent.Inputs)inputs);
        }
    }

    /// <summary>
    /// 入力リスト
    /// </summary>
    public enum Inputs
    {
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3,
        A = 4,
        B = 5,
        L = 6,
        R = 7,
        Esc = 8,
    }
}
