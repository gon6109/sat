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
    public class SaveDataIO : BaseIO
    {
        public string MapPath;
        public string MapName;
        public int SavePointID;
        public List<KeyValuePair<string, int>> EndEvents;
        public List<string> PlayingChacacter;
        public int Time;

        public SaveDataIO()
        {
            MapPath = "";
            MapName = "";
            SavePointID = 0;
            EndEvents = new List<KeyValuePair<string, int>>();
            PlayingChacacter = new List<string>();
            Time = 0;
        }
    }
}
