using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;
using BaseComponent;

namespace SatIO
{
    [XmlRoot("mapData")]
    public class MapIO
    {
        [XmlElement("smallBackGround")]
        public string SmallBackGroundPath { get; set; }
        [XmlElement("largeBackGround")]
        public string LargeBackGroundPath { get; set; }
        [XmlElement("mainMap")]
        public string MainMapPath { get; set; }
        [XmlElement("BGM")]
        public string BGMPath { get; set; }
        [XmlElement("objects")]
        public MapObjects Objects { get; set; }

        public string Path { get; set; }

        public MapIO()
        {
            Objects = new MapObjects();
        }

        /// <summary>
        /// マップを保存
        /// </summary>
        /// <param name="path">ファイル</param>
        public void SaveMap(string path)
        {
            using (FileStream mapfile = new FileStream(path, FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(mapfile, Encoding.UTF8);
                XmlSerializer serializer = new XmlSerializer(typeof(MapIO));
                serializer.Serialize(writer, this);
                writer.Flush();
                writer.Close();
            }
        }

        /// <summary>
        /// マップをロードする
        /// </summary>
        /// <returns>マップデータ（文字列）</returns>
        /// <param name="path">ファイル</param>
        static public MapIO LoadMap(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MapIO));
            MapIO map = (MapIO)serializer.Deserialize(IO.GetStream(path));
            map.Path = System.IO.Path.GetDirectoryName(path);
            return map;
        }
    }

    [XmlRoot("objects")]
    public class MapObjects
    {
        [XmlElement("item")]
        public List<MapItem> MapItems { get; set; }

        public MapObjects()
        {
            MapItems = new List<MapItem>();
        }
    }

    [XmlRoot("item")]
    public class MapItem
    {
        [XmlElement("type")]
        public string Type { get; set; }
        [XmlElement("itemData")]
        public string ItemData { get; set; }
    }
}
