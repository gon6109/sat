using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game.Object.MapEvent
{
    public class MapEventComponent
    {
        public virtual IEnumerator Update() { yield return 0; }
    }
}
