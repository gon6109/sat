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
    public class CharacterImageIO
    {
        public string Name { get; set; }
        public string BaseImagePath { get; set; }
        public Dictionary<string, string> DiffImagePaths { get; set; }

        public CharacterImageIO()
        {
            DiffImagePaths = new Dictionary<string, string>();
        }

        public void SaveCharacterImageIO(string path)
        {
            using (FileStream characterImageFile = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(characterImageFile, this);
            }
        }

        public static async Task<CharacterImageIO> LoadCharacterImageAsync(string path)
        {
            if (path == null || !asd.Engine.File.Exists(path)) return new CharacterImageIO();
            BinaryFormatter serializer = new BinaryFormatter();
            var stream = await IO.GetStreamAsync(path);
            return (CharacterImageIO)serializer.Deserialize(stream);
        }
    }
}
