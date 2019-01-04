using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatIO.MapEventIO
{
    [Serializable()]
    public class MapEventIO
    {
        public List<MapEventComponentIO> Components { get; set; }
        public List<ActorIO> Actors { get; set; }
        public CameraIO Camera { get; set; }
        public List<string> CharacterImagePaths { get; set; }
        
        public VectorIO Position { get; set; }
        public VectorIO Size { get; set; }
        
        public string ToMapPath { get; set; }
        public List<string> PlayerNames { get; set; }
        public VectorIO MoveToPosition { get; set; }
        public int DoorID { get; set; }
        public bool IsUseDoorID { get; set; }
        public int ID { get; set; }

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
            public int ID { get; set; }
            public string Name { get; set; }
            public bool IsUseName { get; set; }
            public VectorIO InitPosition { get; set; }

            public ActorIO()
            {
                Name = "";
            }
        }

        [Serializable()]
        public class CameraIO
        {
            public VectorIO InitPosition { get; set; }
        }
    }
}
