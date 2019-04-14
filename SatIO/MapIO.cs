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
    public class MapIO : BaseIO
    {
        public string MapName;
        public string BGMPath;
        public VectorIO Size;

        public List<CollisionBoxIO> CollisionBoxes;
        public List<CollisionTriangleIO> CollisionTriangles;
        public List<DoorIO> Doors;
        public List<MapObjectIO> MapObjects;
        public List<EventObjectIO> EventObjects;
        public List<MapEventIO.MapEventIO> MapEvents;
        public List<BackGroundIO> BackGrounds;
        public List<CameraRestrictionIO> CameraRestrictions;
        public List<SavePointIO> SavePoints;

        public MapIO()
        {
            CollisionBoxes = new List<CollisionBoxIO>();
            CollisionTriangles = new List<CollisionTriangleIO>();
            Doors = new List<DoorIO>();
            MapObjects = new List<MapObjectIO>();
            EventObjects = new List<EventObjectIO>();
            MapEvents = new List<MapEventIO.MapEventIO>();
            BackGrounds = new List<BackGroundIO>();
            CameraRestrictions = new List<CameraRestrictionIO>();
            SavePoints = new List<SavePointIO>();
            Size = new VectorIO();
        }

        /// <summary>
        /// マップの要素の総数を得る
        /// </summary>
        /// <returns>マップの要素の総数</returns>
        public int GetMapElementCount()
        {
            return CollisionBoxes.Count + CollisionTriangles.Count + Doors.Count + MapObjects.Count + MapEvents.Count + BackGrounds.Count + SavePoints.Count;
        }
    }
}
