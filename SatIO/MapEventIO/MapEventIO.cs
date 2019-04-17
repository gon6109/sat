using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SatIO.MapEventIO
{
    [Serializable()]
    public class MapEventIO
    {
        [XmlArrayItem(typeof(MoveComponentIO))]
        [XmlArrayItem(typeof(TalkComponentIO))]
        public List<MapEventComponentIO> Components;
        public List<ActorIO> Actors;
        public CameraIO Camera;
        public List<string> CharacterImagePaths;
        
        public VectorIO Position;
        public VectorIO Size;
        
        public string ToMapPath;
        public List<string> PlayerNames;
        public VectorIO MoveToPosition;
        public int DoorID;
        public bool IsUseDoorID;
        public int ID;

        public MapEventIO()
        {
            Components = new List<MapEventComponentIO>();
            Actors = new List<ActorIO>();
            CharacterImagePaths = new List<string>();
            Camera = new CameraIO();
        }

        [Serializable()]
        public class ActorIO
        {
            public int ID;
            public string Name;
            public bool IsUseName;
            public VectorIO InitPosition;

            public ActorIO()
            {
                Name = "";
            }
        }

        [Serializable()]
        public class CameraIO
        {
            public VectorIO InitPosition;
        }
    }
}
