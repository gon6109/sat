using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatIO
{
    [Serializable()]
    public class EventObjectIO : MapObjectIO
    {
        public string MotionPath { get; set; }
        public int ID { get; set; }
    }
}
