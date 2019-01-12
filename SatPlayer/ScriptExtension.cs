using AlteseedScript.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    public static class ScriptExtension
    {
        /// <summary>
        /// Vector2DF型に変換する。
        /// </summary>
        /// <param name="vector">Vector型</param>
        /// <returns>Vector2DF型</returns>
        public static asd.Vector2DF ToAsdVector(this Vector vector)
        {
            return new asd.Vector2DF(vector.X, vector.Y);
        }

        /// <summary>
        /// Vector型に変換する
        /// </summary>
        /// <param name="vector">Vector2DF型</param>
        /// <returns>Vector型</returns>
        public static Vector ToScriptVector(this asd.Vector2DF vector)
        {
            return new Vector(vector.X, vector.Y);
        }
    }
}
