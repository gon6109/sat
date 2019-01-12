using BaseComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SatIO
{
    [Serializable()]
    public class MapObjectTemplateIO : BaseIO
    {
        public SerializableDictionary<string, EventObjectIO> Templates { get; set; }

        public MapObjectTemplateIO()
        {
            Templates = new SerializableDictionary<string, EventObjectIO>();
        }
    }
}
