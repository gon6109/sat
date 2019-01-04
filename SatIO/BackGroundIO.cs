using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatIO
{
    [Serializable]
    public class BackGroundIO
    {
        public string TexturePath { get; set; }
        public VectorIO Position { get; set; }
        public float Zoom { get; set; }
    }
}
