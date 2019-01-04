using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BaseComponent;

namespace SatIO
{
    [Serializable()]
    public class BinaryMapIO
    {
        public string MapName;
        public string BGMPath;
        public string Path;
        public VectorIO Size;

        public List<CollisionBoxIO> CollisionBoxes;
        public List<CollisionTriangleIO> CollisionTriangles;
        public List<DoorIO> Doors;
        public List<MapObjectIO> MapObjects;
        public List<NPCMapObjectIO> NPCMapObjects;
        public List<MapEventIO.MapEventIO> MapEvents;
        public List<BackGroundIO> BackGrounds;
        public List<CameraRestrictionIO> CameraRestrictions;
        public List<SavePointIO> SavePoints;

        public BinaryMapIO()
        {
            CollisionBoxes = new List<CollisionBoxIO>();
            CollisionTriangles = new List<CollisionTriangleIO>();
            Doors = new List<DoorIO>();
            MapObjects = new List<MapObjectIO>();
            NPCMapObjects = new List<NPCMapObjectIO>();
            MapEvents = new List<MapEventIO.MapEventIO>();
            BackGrounds = new List<BackGroundIO>();
            CameraRestrictions = new List<CameraRestrictionIO>();
            SavePoints = new List<SavePointIO>();
            Size = new VectorIO();
        }

        /// <summary>
        /// マップを保存
        /// </summary>
        /// <param name="path">ファイル</param>
        public void SaveMap(string path)
        {
            using (FileStream mapfile = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(mapfile, this);
            }
        }

        /// <summary>
        /// マップをロードする
        /// </summary>
        /// <returns>マップデータ（文字列）</returns>
        /// <param name="path">ファイル</param>
        static public BinaryMapIO LoadMap(string path)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            BinaryMapIO map = (BinaryMapIO)serializer.Deserialize(IO.GetStream(path));
            map.Path = System.IO.Path.GetDirectoryName(path);
            return map;
        }
    }
}
