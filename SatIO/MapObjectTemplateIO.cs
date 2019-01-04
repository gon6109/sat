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
    public class MapObjectTemplateIO
    {
        public Dictionary<string, NPCMapObjectIO> Templates { get; set; }

        public MapObjectTemplateIO()
        {
            Templates = new Dictionary<string, NPCMapObjectIO>();
        }

        public void SaveMapObjectTemplateIO(string path)
        {
            using (FileStream templateFile = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(templateFile, this);
            }
        }

        public static MapObjectTemplateIO LoadMapObjectTemplate(string path)
        {
            if (!asd.Engine.File.Exists(path)) return new MapObjectTemplateIO();
            BinaryFormatter serializer = new BinaryFormatter();
            return (MapObjectTemplateIO)serializer.Deserialize(IO.GetStream(path));
        }
    }
}
