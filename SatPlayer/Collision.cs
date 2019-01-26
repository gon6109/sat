using SatScript.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    public struct Collision : ICollision
    {
        public bool IsCollidedWithObstacle { get; set; }

        public bool IsCollidedWithPlayer { get; set; }

        public IEnumerable<string> ColligingMapObjectTags { get; set; }
    }
}
