using BaseComponent;
using SatIO.MapEventIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.MapEvent
{
    public class MoveComponent : MapEventComponent
    {

        public Dictionary<MapEvent.Actor, CharacterMoveCommand> Commands { get; set; }
        public CharacterMoveCommand CameraCommand { get; set; }
        
        public int Frame { get; set; }
        
        public List<MapEvent.Actor> Actors { get; set; }

        public ScrollCamera MainCamera { get; set; }

        public MoveComponent(List<MapEvent.Actor> actors, ScrollCamera camera)
        {
            Commands = new Dictionary<MapEvent.Actor, CharacterMoveCommand>();
            CameraCommand = new CharacterMoveCommand();
            Actors = actors;
            MainCamera = camera;
        }

        public static MoveComponent LoadMoveComponent(MoveComponentIO moveComponentIO, List<MapEvent.Actor> actors, ScrollCamera camera)
        {
            var component = new MoveComponent(actors, camera);
            component.Frame = moveComponentIO.Frame;
            foreach (var item in moveComponentIO.Commands)
            {
                component.Commands[actors.Where(obj => obj.Motion.IsUseName ? obj.Motion.Name == item.Key.Name : obj.Motion.ID == item.Key.ID).First()]
                    = new CharacterMoveCommand() { MoveCommandElements = item.Value.MoveCommandElements };
            }
            if (moveComponentIO.CameraCommand != null)
                component.CameraCommand = new CharacterMoveCommand() { MoveCommandElements = moveComponentIO.CameraCommand.MoveCommandElements };

            return component;
        }

        public override IEnumerator Update()
        {
            for (int i = 0; i < Frame; i++)
            {
                foreach (var item in Actors)
                {
                    if (!Commands.ContainsKey(item) ||
                                Commands[item].MoveCommandElements.Count <= i) continue;
                    item.Motion.MoveCommands.Enqueue(Commands[item].MoveCommandElements[i]);
                }
                if (CameraCommand.MoveCommandElements.Count > i)
                    MainCamera.MoveCommands.Enqueue(CameraCommand.MoveCommandElements[i]);
                yield return 0;
            }
            for (int i = 0; i < 30; i++)
            {
                foreach (var item in Actors)
                {
                    item.Motion.MoveCommands.Enqueue(new Dictionary<Inputs, bool>());
                }

                yield return 0;
            }
            yield return 0;
        }

        public class CharacterMoveCommand
        {
            public List<Dictionary<Inputs, bool>> MoveCommandElements { get; set; }

            public CharacterMoveCommand()
            {
                MoveCommandElements = new List<Dictionary<Inputs, bool>>();
            }
        }
    }
}
