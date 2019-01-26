using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltseedScript.Common
{
    /// <summary>
    /// ベクトル
    /// </summary>
    public struct Vector
    {
        /// <summary>
        /// X成分
        /// </summary>
        public float X;

        /// <summary>
        /// Y成分
        /// </summary>
        public float Y;

        /// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="x">X成分</param>
		/// <param name="y">Y成分</param>
        public Vector(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// ベクトルの長さを取得または設定する。
        /// </summary>
        public float Length
        {
            get { return (float)Math.Sqrt(SquaredLength); }
            set
            {
                float angle = Radian;
                X = (float)Math.Cos(angle) * value;
                Y = (float)Math.Sin(angle) * value;
            }
        }

        /// <summary>
        /// ベクトルの長さの二乗を取得する。
        /// </summary>
        public float SquaredLength
        {
            get { return X * X + Y * Y; }
        }

        /// <summary>
        /// このベクトルの単位ベクトルを取得する。
        /// </summary>
        public Vector Normal
        {
            get
            {
                float length = Length;
                return new Vector(X / length, Y / length);
            }
        }

        /// <summary>
        /// このベクトルを単位ベクトル化する。
        /// </summary>
        public void Normalize()
        {
            float length = Length;
            X /= length;
            Y /= length;
        }

        /// <summary>
        /// ベクトルの向きを弧度法で取得または設定する。
        /// </summary>
        public float Radian
        {
            get => (float)Math.Atan2(Y, X);
            set
            {
                float length = Length;
                X = (float)Math.Cos(value) * length;
                Y = (float)Math.Sin(value) * length;
            }
        }
        /// <summary>
        /// ベクトルの向きを度数法で取得または設定する。
        /// </summary>
        public float Degree
        {
            get => asd.MathHelper.RadianToDegree(Radian);
            set => Radian = asd.MathHelper.DegreeToRadian(value);
        }

        public bool Equals(Vector other)
        {
            return X == other.X && Y == other.Y;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }

        public static bool operator ==(Vector left, Vector right) => left.X == right.X && left.Y == right.Y;
        public static bool operator !=(Vector left, Vector right) => left.X != right.X || left.Y != right.Y;
        public static Vector operator +(Vector left, Vector right) => new Vector(left.X + right.X, left.Y + right.Y);
        public static Vector operator -(Vector left, Vector right) => new Vector(left.X - right.X, left.Y - right.Y);
        public static Vector operator -(Vector op) => new Vector(-op.X, -op.Y);
        public static Vector operator *(Vector op, float scolar) => new Vector(op.X * scolar, op.Y * scolar);
        public static Vector operator *(float scolar, Vector op) => new Vector(scolar * op.X, scolar * op.Y);
        public static Vector operator *(Vector left, Vector right) => new Vector(left.X * right.X, left.Y * right.Y);
        public static Vector operator /(Vector op, float scolar) => new Vector(op.X / scolar, op.Y / scolar);
        public static Vector operator /(Vector left, Vector right) => new Vector(left.X / right.X, left.Y / right.Y);

        /// <summary>
        /// 外積を取得する。
        /// </summary>
        /// <param name="v1">v1ベクトル</param>
        /// <param name="v2">v2ベクトル</param>
        /// <returns>外積v1×v2</returns>
        public static float Cross(Vector v1, Vector v2) => v1.X * v2.Y - v1.Y * v2.X;

        /// <summary>
        /// 内積を取得する。
        /// </summary>
        /// <param name="v1">v1ベクトル</param>
        /// <param name="v2">v2ベクトル</param>
        /// <returns>内積v1・v2</returns>
        public static float Dot(Vector v1, Vector v2) => v1.X * v2.X + v1.Y * v2.Y;

        /// <summary>
        /// 2点間の距離を取得する。
        /// </summary>
        /// <param name="v1">v1ベクトル</param>
        /// <param name="v2">v2ベクトル</param>
        /// <returns>距離</returns>
        public static float Distance(Vector v1, Vector v2)
        {
            float dx = v1.X - v2.X;
            float dy = v1.Y - v2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 加算する。
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector Add(Vector v1, Vector v2)
        {
            Vector o = new Vector();
            o.X = v1.X + v2.X;
            o.Y = v1.Y + v2.Y;
            return o;
        }

        /// <summary>
        /// 減算する。
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector Subtract(Vector v1, Vector v2)
        {
            Vector o = new Vector();
            o.X = v1.X - v2.X;
            o.Y = v1.Y - v2.Y;
            return o;
        }

        /// <summary>
        /// 除算する。
        /// </summary>
        /// <param name="v1">値1</param>
        /// <param name="v2">値2</param>
        /// <returns>v1/v2</returns>
        public static Vector Divide(Vector v1, Vector v2)
        {
            var ret = new Vector();
            ret.X = v1.X / v2.X;
            ret.Y = v1.Y / v2.Y;
            return ret;
        }

        /// <summary>
        /// スカラーで除算する。
        /// </summary>
        /// <param name="v1">値1</param>
        /// <param name="v2">値2</param>
        /// <returns>v1/v2</returns>
        public static Vector DivideByScalar(Vector v1, float v2)
        {
            var ret = new Vector();
            ret.X = v1.X / v2;
            ret.Y = v1.Y / v2;
            return ret;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
    }
}
