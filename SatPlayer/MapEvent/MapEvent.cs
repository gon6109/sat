using BaseComponent;
using SatIO.MapEventIO;
using PhysicAltseed;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.MapEvent
{
    /// <summary>
    /// イベント
    /// </summary>
    public class MapEvent : asd.GeometryObject2D
    {
        public new asd.RectangleShape Shape
        {
            get => (asd.RectangleShape)base.Shape;
            private set => base.Shape = value;
        }

        public int ID { get; private set; }

        public List<Actor> Actors;

        public List<CharacterImage> CharacterImages { get; set; }

        public List<MapEventComponent> EventComponents { get; set; }

        public ScrollCamera MainCamera { get; private set; }

        public asd.Vector2DF InitCameraPosition { get; set; }

        public string ToMapPath { get; set; }

        public List<PlayerName> PlayerNames { get; set; }

        public asd.Vector2DF MoveToPosition { get; set; }

        public int DoorID { get; set; }

        public bool IsUseDoorID { get; set; }

        IEnumerator enumerator;

        public MapEvent(MapEventIO mapEventIO, List<IActor> allActors, ScrollCamera camera)
        {
            Shape = new asd.RectangleShape();

            Actors = new List<Actor>();
            EventComponents = new List<MapEventComponent>();
            CharacterImages = new List<CharacterImage>();
            MainCamera = camera;

            PlayerNames = new List<PlayerName>();
            IsDrawn = false;
            IsUpdated = false;

            LoadMapEventIO(mapEventIO, allActors);
        }

        void LoadMapEventIO(MapEventIO mapEventIO, List<IActor> allActors)
        {
            ID = mapEventIO.ID;
            Shape.DrawingArea = new asd.RectF(mapEventIO.Position, mapEventIO.Size);
            InitCameraPosition = mapEventIO.Camera.InitPosition + new asd.Vector2DF(400,300);
            ToMapPath = mapEventIO.ToMapPath;
            MoveToPosition = mapEventIO.MoveToPosition;
            DoorID = mapEventIO.DoorID;
            IsUseDoorID = mapEventIO.IsUseDoorID;
            if (mapEventIO.PlayerNames != null)
            {
                foreach (var item in mapEventIO.PlayerNames)
                {
                    PlayerNames.Add(new PlayerName() { Name = item });
                }
            }
            foreach (var item in mapEventIO.Actors)
            {
                try
                {
                    var actor = new Actor();
                    actor.ActorObject = allActors.First(obj => obj.IsUseName ? obj.Name == item.Name : obj.ID == item.ID);
                    actor.InitPosition = item.InitPosition;
                    Actors.Add(actor);
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
            }
            foreach (var item in mapEventIO.CharacterImagePaths)
            {
                CharacterImages.Add(CharacterImage.LoadCharacterImage(item));
            }
            foreach (var item in mapEventIO.Components)
            {
                if (item is MoveComponentIO)
                {
                    var component = MoveComponent.LoadMoveComponent((MoveComponentIO)item, Actors, MainCamera);
                    EventComponents.Add(component);
                }
                if (item is TalkComponentIO)
                {
                    var component = TalkComponent.LoadTalkComponent((TalkComponentIO)item, CharacterImages);
                    EventComponents.Add(component);
                }
            }
        }
        int counter = 0;

        protected override void OnUpdate()
        {
            if (enumerator == null) enumerator = Init();
            
            var result = enumerator.MoveNext();
            if (!result)
            {
                if (enumerator == Init()) enumerator = EventComponents[counter++].Update();
                else
                {
                    if (EventComponents.Count > counter) enumerator = EventComponents[counter++].Update();
                    else enumerator = Close();
                }
            }
        }

        private IEnumerator Close()
        {
            MainCamera.IsEvent = false;
            if (ToMapPath != null && asd.Engine.File.Exists(ToMapPath))
            {
                var scene = asd.Engine.CurrentScene as Game;
                scene?.OnChangeMapEvent
                    (
                    ToMapPath,
                    Game.Players.FindAll(obj => PlayerNames.Any(name => name.Name == obj.Name)).ToList(),
                    MoveToPosition,
                    IsUseDoorID ? DoorID : -1
                    );
            }

            int count = 0;
            while (count < 30)
            {
                foreach (MapObject item in Layer.Objects.Where(obj => obj is MapObject))
                {
                    if (item.Color.A == 255) continue;
                    item.Color = new asd.Color(255, 255, 255, item.Color.A + 8);
                }
                foreach (var item in Actors)
                {
                    if (item.ActorObject is EventObject) ((EventObject)item.ActorObject).Color = new asd.Color(255, 255, 255, 255);
                }
                count++;
                yield return 0;
            }

            foreach (MapObject item in Layer.Objects.Where(obj => obj is MapObject))
            {
                item.IsUpdated = true;
                if (item.CollisionShape != null) item.CollisionShape.IsActive = true;
                item.Color = new asd.Color(255, 255, 255, 255);
            }
            foreach (var item in Actors)
            {
                if (item.ActorObject is EventObject)
                {
                    ((EventObject)item.ActorObject).CollisionShape.GroupIndex = 0;
                }
                if (item.ActorObject is Player)
                {
                    List<IActor> actors = new List<IActor>();
                    ((Player)item.ActorObject).CollisionShape.GroupIndex = -1;
                }
                item.ActorObject.IsEvent = false;
            }
            var path = (Layer.Scene as Game)?.MapPath;
            Game.EndEvents.Add(new KeyValuePair<string, int>(path, ID));
            Dispose();
            yield return 0;
        }

        IEnumerator Init()
        {
            foreach (MapObject item in Layer.Objects.Where(obj => obj is MapObject))
            {
                item.IsUpdated = false;
                if (item.CollisionShape != null) item.CollisionShape.IsActive = false;
            }
            foreach (var item in Actors)
            {
                if (item.ActorObject is EventObject eventObject)
                {
                    eventObject.IsUpdated = true;
                    eventObject.CollisionShape.IsActive = true;
                    eventObject.CollisionShape.GroupIndex = -1;
                    eventObject.Position = item.InitPosition;
                }
                item.ActorObject.IsEvent = true;
            }
            MainCamera.IsEvent = true;
            MainCamera.WaitStatePoints.Enqueue(InitCameraPosition);

            int count = 0;
            while (Actors.Any(obj => (obj.InitPosition - obj.ActorObject.Position).Length > 5) || count < 256)
            {
                foreach (MapObject item in Layer.Objects.Where(obj => obj is MapObject))
                {
                    if (item.Color.A == 0) continue;
                    item.Color = new asd.Color(255, 255, 255, item.Color.A - 1);
                }
                foreach (var item in Actors)
                {
                    if (item.ActorObject is EventObject) ((EventObject)item.ActorObject).Color = new asd.Color(255, 255, 255, 255);
                    if (item.ActorObject is Player)
                    {
                        var command = new Dictionary<Inputs, bool>();
                        if ((item.ActorObject.Position - item.InitPosition).Length < 5) continue;
                        else if (item.ActorObject.Position.X > item.InitPosition.X) command.Add(Inputs.Left, true);
                        else if (item.ActorObject.Position.X < item.InitPosition.X) command.Add(Inputs.Right, true);
                        item.ActorObject.MoveCommands.Enqueue(command);
                    }
                }
                count++;
                yield return 0;
            }
            yield return 0;
        }

        public class Actor
        {
            public IActor ActorObject { get; set; }

            public asd.Vector2DF InitPosition { get; set; }
        }

        public class PlayerName
        {
            public string Name { get; set; }
        }
    }
}
