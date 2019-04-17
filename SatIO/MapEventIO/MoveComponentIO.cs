using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SatIO.MapEventIO
{
    [Serializable()]
    public class MoveComponentIO : MapEventComponentIO
    {
        public SerializableDictionary<MapEventIO.ActorIO, CharacterMoveCommandIO> Commands;
        public CharacterMoveCommandIO CameraCommand;
        public int Frame;

        public MoveComponentIO()
        {
            Commands = new SerializableDictionary<MapEventIO.ActorIO, CharacterMoveCommandIO>();
            CameraCommand = new CharacterMoveCommandIO();
        }

        [Serializable()]
        public class CharacterMoveCommandIO
        {
            public List<SerializableDictionary<Inputs, bool>> MoveCommandElements;

            public CharacterMoveCommandIO()
            {
                MoveCommandElements = new List<SerializableDictionary<Inputs, bool>>();
            }
        }
    }
}
