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
        public List<NPCMapObjectIO> NPCMapObjects;
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
            NPCMapObjects = new List<NPCMapObjectIO>();
            MapEvents = new List<MapEventIO.MapEventIO>();
            BackGrounds = new List<BackGroundIO>();
            CameraRestrictions = new List<CameraRestrictionIO>();
            SavePoints = new List<SavePointIO>();
            Size = new VectorIO();
        }
    }
}
