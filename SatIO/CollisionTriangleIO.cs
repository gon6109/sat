using System;
using System.Collections.Generic;
using System.Text;

namespace SatIO
{
    [Serializable()]
    public class CollisionTriangleIO
    {
        public VectorIO[] vertexes;

        public CollisionTriangleIO()
        {
            vertexes = new VectorIO[3];
        }
    }
}
