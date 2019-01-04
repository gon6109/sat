using System;
using System.Collections.Generic;
using System.Text;

namespace SatIO
{
    [Serializable()]
    public class VectorIO
    {
        public float X;
        public float Y;
        
        public static implicit operator asd.Vector2DF(VectorIO vector)
        {
            return vector != null ? new asd.Vector2DF(vector.X, vector.Y) : new asd.Vector2DF();
        }

        public static implicit operator VectorIO(asd.Vector2DF vector)
        {
            return new VectorIO() { X = vector.X, Y = vector.Y };
        }
    }
}
