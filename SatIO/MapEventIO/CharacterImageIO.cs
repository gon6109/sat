using BaseComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SatIO.MapEventIO
{
    [Serializable()]
    public class CharacterImageIO : BaseIO
    {
        public string Name;
        public string BaseImagePath;
        public SerializableDictionary<string, string> DiffImagePaths;

        public CharacterImageIO()
        {
            DiffImagePaths = new SerializableDictionary<string, string>();
        }
    }
}
