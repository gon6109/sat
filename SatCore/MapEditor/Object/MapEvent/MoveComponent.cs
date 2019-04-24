using BaseComponent;
using SatCore.Attribute;
using SatCore.MapEditor.Object.MapEvent;
using SatIO.MapEventIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor.Object.MapEvent
{
    /// <summary>
    /// キャラ動作系
    /// </summary>
    public class MoveComponent : MapEventComponent
    {
        public new string Name => "Move :" + Frame;

        public Dictionary<MapEvent.Actor, CharacterMoveCommand> Commands { get; set; }
        public CharacterMoveCommand CameraCommand { get; set; }

        [NumberInput("フレーム長")]
        public int Frame
        {
            get => _frame;
            set
            {
                if (value < 1) return;
                UndoRedoManager.ChangeProperty(this, value);
                _frame = value;
                OnPropertyChanged();
                OnPropertyChanged("Name");
            }
        }

        [ListInput("登場キャラ", "SelectedActor", isVisibleRemoveButtton: false)]
        public ObservableCollection<MapEvent.Actor> Actors { get; set; }

        public MapEvent.Actor SelectedActor { get; set; }

        public MapEvent.Camera MainCamera { get; set; }

        IEnumerator iterator;
        private int _frame;

        [Button("カメラを動かす")]
        public void MoveCamera()
        {
            if (iterator != null) return;
            iterator = SetCommand(MainCamera);
            foreach (var item in Actors)
            {
                item.Active = true;
            }
            MainCamera.Active = true;
        }

        [Button("選択キャラを動かす")]
        public void MoveSelectedCharacter()
        {
            if (iterator != null || SelectedActor == null) return;
            iterator = SetCommand(SelectedActor);
            foreach (var item in Actors)
            {
                item.Active = true;
            }
            MainCamera.Active = true;
        }

        public MoveComponent(ObservableCollection<MapEvent.Actor> actors, MapEvent.Camera camera)
        {
            Commands = new Dictionary<MapEvent.Actor, CharacterMoveCommand>();
            CameraCommand = new CharacterMoveCommand();
            Actors = actors;
            MainCamera = camera;
            Frame = 60;
        }

        public static MoveComponent LoadMoveComponent(MoveComponentIO moveComponentIO, ObservableCollection<MapEvent.Actor> actors, MapEvent.Camera camera)
        {
            var component = new MoveComponent(actors, camera);
            component.Frame = moveComponentIO.Frame;
            foreach (var item in moveComponentIO.Commands)
            {
                try
                {
                    component.Commands[actors.Where(obj => (obj.Path != null && obj.Path == item.Key.Path) ? true : obj.ID == item.Key.ID).First()]
                        = new CharacterMoveCommand() { MoveCommandElements = item.Value.MoveCommandElements.Select(obj => new Dictionary<Inputs, bool>(obj)).ToList() };
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
            }
            if (moveComponentIO.CameraCommand != null)
                component.CameraCommand = new CharacterMoveCommand() { MoveCommandElements = moveComponentIO.CameraCommand.MoveCommandElements.Select(obj => new Dictionary<Inputs, bool>(obj)).ToList() };

            return component;
        }

        public static explicit operator MoveComponentIO(MoveComponent moveComponent)
        {
            MoveComponentIO moveComponentIO = new MoveComponentIO()
            {
                Frame = moveComponent.Frame,
                Commands = new SatIO.SerializableDictionary<MapEventIO.ActorIO, MoveComponentIO.CharacterMoveCommandIO>(moveComponent.Commands.ToDictionary(
                    obj => (MapEventIO.ActorIO)obj.Key,
                    obj => new MoveComponentIO.CharacterMoveCommandIO() { MoveCommandElements = obj.Value.MoveCommandElements.Select(obj2 => new SatIO.SerializableDictionary<Inputs, bool>(obj2)).ToList() })),
                CameraCommand = new MoveComponentIO.CharacterMoveCommandIO() { MoveCommandElements = moveComponent.CameraCommand.MoveCommandElements.Select(obj => new SatIO.SerializableDictionary<Inputs, bool>(obj)).ToList() },
            };
            return moveComponentIO;
        }

        public override void Update()
        {
            if (iterator != null)
            {
                if (!iterator.MoveNext()) iterator = null;
            }
        }

        public IEnumerator SetCommand(MapEvent.Actor actor)
        {
            var initCameraPos = MainCamera.Position;
            var initActorPos = actor.Position;
            var actorsInitPos = Actors.ToDictionary(obj => obj, obj => obj.Position);
            var before = Commands.ContainsKey(actor) ? Commands[actor].MoveCommandElements : new List<Dictionary<Inputs, bool>>();
            if (Commands.ContainsKey(actor)) Commands[actor].MoveCommandElements = new List<Dictionary<Inputs, bool>>(before);

            foreach (var item in Actors)
            {
                if (item == actor) continue;
                item.IsSimulateEvent = true;
            }

            for (int i = 0; i < Frame; i++)
            {
                Dictionary<Inputs, bool> moveCommandElements = new Dictionary<Inputs, bool>();

                foreach (Inputs item in Enum.GetValues(typeof(Inputs)))
                {
                    moveCommandElements[item] = Input.GetInputState(item) > 0;
                }

                if (!Commands.ContainsKey(actor))
                {
                    Commands[actor] = new CharacterMoveCommand();
                    Commands[actor].MoveCommandElements.Add(moveCommandElements);
                }
                else if (Commands[actor].MoveCommandElements.Count > i) Commands[actor].MoveCommandElements[i] = moveCommandElements;
                else Commands[actor].MoveCommandElements.Add(moveCommandElements);

                foreach (var item in Actors)
                {
                    if (item == actor) continue;
                    if (!Commands.ContainsKey(item) ||
                                Commands[item].MoveCommandElements.Count <= i) item.AddRequest(new Dictionary<Inputs, bool>());
                    else item.AddRequest(Commands[item].MoveCommandElements[i]);
                }
                if (CameraCommand.MoveCommandElements.Count > i)
                    MainCamera.AddRequest(CameraCommand.MoveCommandElements[i]);
                else MainCamera.AddRequest(new Dictionary<Inputs, bool>());

                if (i == Frame - 1)
                {
                    foreach (var item in Actors)
                    {
                        if (item == actor) continue;
                        item.Active = false;
                        item.IsSimulateEvent = false;
                        item.Position = actorsInitPos[item];
                    }
                    MainCamera.Active = false;
                    MainCamera.Position = initCameraPos;
                    actor.Active = false;
                    actor.ClearTexture();
                    actor.SetTexture(initActorPos, new asd.Color(100, 255, 100));
                    actor.SetTexture(actor.Position, new asd.Color(255, 100, 100));
                    actor.Position = initActorPos;
                    UndoRedoManager.ChangeProperty(Commands[actor], Commands[actor].MoveCommandElements, before, "MoveCommandElements");
                }

                yield return 0;
            }
        }

        public IEnumerator SetCommand(MapEvent.Camera camera)
        {
            var initPos = camera.Position;
            var actorsInitPos = Actors.ToDictionary(obj => obj, obj => obj.Position);
            var before = CameraCommand.MoveCommandElements;
            CameraCommand.MoveCommandElements = new List<Dictionary<Inputs, bool>>(before);

            foreach (var item in Actors)
            {
                item.IsSimulateEvent = true;
            }

            for (int i = 0; i < Frame; i++)
            {
                Dictionary<Inputs, bool> moveCommandElements = new Dictionary<Inputs, bool>();

                foreach (Inputs item in Enum.GetValues(typeof(Inputs)))
                {
                    moveCommandElements[item] = Input.GetInputState(item) > 0;
                }

                if (CameraCommand.MoveCommandElements.Count > i) CameraCommand.MoveCommandElements[i] = moveCommandElements;
                else CameraCommand.MoveCommandElements.Add(moveCommandElements);

                foreach (var item in Actors)
                {
                    if (!Commands.ContainsKey(item) ||
                                Commands[item].MoveCommandElements.Count <= i) item.AddRequest(new Dictionary<Inputs, bool>());
                    else item.AddRequest(Commands[item].MoveCommandElements[i]);
                }

                if (i == Frame - 1)
                {
                    foreach (var item in Actors)
                    {
                        item.Active = false;
                        item.IsSimulateEvent = false;
                        item.Position = actorsInitPos[item];
                    }
                    camera.Active = false;
                    camera.ClearGeometry();
                    camera.SetGeometry(camera.Layer, initPos, new asd.Color(255, 0, 0, 100));
                    camera.SetGeometry(camera.Layer, camera.Position, new asd.Color(255, 255, 0, 100));
                    camera.Position = initPos;
                    UndoRedoManager.ChangeProperty(CameraCommand, CameraCommand.MoveCommandElements, before, "MoveCommandElements");
                }
                yield return 0;
            }
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
