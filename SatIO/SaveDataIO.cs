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
    [Serializable]
    public class SaveSatIO
    {
        public string MapPath;
        public string MapName;
        public int SavePointID;
        public List<KeyValuePair<string, int>> EndEvents;
        public List<string> PlayingChacacter;
        public int Time;

        public SaveSatIO()
        {
            MapPath = "";
            MapName = "";
            SavePointID = 0;
            EndEvents = new List<KeyValuePair<string, int>>();
            PlayingChacacter = new List<string>();
            Time = 0;
        }

        /// <summary>
        /// セーブデータを保存
        /// </summary>
        /// <param name="path">ファイル</param>
        public void Save(string path)
        {
            using (FileStream mapfile = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(mapfile, this);
            }
        }

        /// <summary>
        /// セーブデータをロードする
        /// </summary>
        /// <returns>マップデータ（文字列）</returns>
        /// <param name="path">ファイル</param>
        static public SaveSatIO LoadSaveData(string path)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            SaveSatIO saveData = (SaveSatIO)serializer.Deserialize(IO.GetStream(path));
            return saveData;
        }
    }
}
