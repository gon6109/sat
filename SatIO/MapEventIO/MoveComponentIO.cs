using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatIO.MapEventIO
{
    [Serializable()]
    public class MoveComponentIO : MapEventComponentIO
    {
        public Dictionary<MapEventIO.ActorIO, CharacterMoveCommandIO> Commands { get; set; }
        public CharacterMoveCommandIO CameraCommand { get; set; }
        public int Frame { get; set; }

        public MoveComponentIO()
        {
            Commands = new Dictionary<MapEventIO.ActorIO, CharacterMoveCommandIO>();
            CameraCommand = new CharacterMoveCommandIO();
        }

        [Serializable()]
        public class CharacterMoveCommandIO
        {
            public List<Dictionary<Inputs, bool>> MoveCommandElements { get; set; }

            public CharacterMoveCommandIO()
            {
                MoveCommandElements = new List<Dictionary<Inputs, bool>>();
            }
        }
    }
}
