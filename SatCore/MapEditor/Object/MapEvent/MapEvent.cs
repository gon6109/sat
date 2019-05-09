using BaseComponent;
using SatIO.MapEventIO;
using PhysicAltseed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SatPlayer.Game.Object;
using SatPlayer.Game;
using SatCore.Attribute;

namespace SatCore.MapEditor.Object.MapEvent
{
    /// <summary>
    /// 強制イベント
    /// </summary>
    public class MapEvent : asd.GeometryObject2D, INotifyPropertyChanged, IMovable, IMapElement
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private MapEventComponent _selectedComponent;

        public new asd.RectangleShape Shape
        {
            get
            {
                base.Shape = base.Shape;
                return (asd.RectangleShape)base.Shape;
            }
            set => base.Shape = value;
        }

        [TextOutput("ID")]
        public int ID { get; set; }

        [VectorInput("開始領域左上座標")]
        public new asd.Vector2DF Position
        {
            get => Shape.DrawingArea.Position;
            set
            {
                Shape.DrawingArea = new asd.RectF(value, Shape.DrawingArea.Size);
                OnPropertyChanged();
            }
        }

        [VectorInput("開始領域サイズ")]
        public asd.Vector2DF Size
        {
            get => Shape.DrawingArea.Size;
            set
            {
                Shape.DrawingArea = new asd.RectF(Shape.DrawingArea.Position, value);
                OnPropertyChanged();
            }
        }

        [ListInput("動かすキャラ", additionButtonEventMethodName: "AddActor")]
        public UndoRedoCollection<Actor> Actors { get; set; }

        [Group("カメラ")]
        public Camera MainCamera { get; set; }

        public void AddActor()
        {
            if (((MapLayer)Layer).CurrentToolType != ToolType.Select ||
                ((MapLayer)Layer).Objects.Count(obj => obj is EventObject) <= Actors.Count(obj => obj.Path == null))
                return;
            ((MapLayer)Layer).CurrentToolType = ToolType.SelectEventObject;
        }

        public void AddEventObjectActor(EventObject actorObject, int id, asd.Vector2DF initPosition)
        {
            var actor = new Actor(actorObject);
            actor.InitPosition = actorObject.StartPosition;
            Actors.Add(actor);
            actor.Active = true;

            if (Layer is MapLayer map)
                for (int i = 0; i < 60; i++)
                {
                    UpdateCollision();
                    map.PhysicalWorld.Update();
                    actor.Update();
                }

            actor.InitPosition = actorObject.Position;
            actor.Active = false;
            actorObject.IsActivePhysic = true;

            actor.SetTexture(actor.Position, new asd.Color(255, 255, 255));
        }

        [Button("プレイヤーを登録")]
        public async Task AddPlayerActorAsync()
        {
            try
            {
                if (PlayersListDialog.GetPlayersScriptPaths().Count() <= Actors.Count(obj => obj.Path != null)) return;

                PlayersListDialog playersListDialog = new PlayersListDialog();
                if (playersListDialog.Show() != PlayersListDialogResult.OK) return;

                var actor = new Actor(await MapEventPlayer.CreatePlayerAsync(playersListDialog.FileName));

                actor.InitPosition = Position + new asd.Vector2DF(Size.X, 0) - actor.Texture?.Size.To2DF() ?? new asd.Vector2DF();
                actor.Position = actor.InitPosition;
                Actors.Add(actor);
                actor.Active = true;

                if (Layer is MapLayer map)
                    for (int i = 0; i < 60; i++)
                    {
                        UpdateCollision();
                        map.PhysicalWorld.Update();
                        actor.Update();
                    }

                actor.InitPosition = actor.Position;

                actor.Active = false;

                actor.SetTexture(actor.InitPosition, new asd.Color(255, 255, 255));
                var playerName = new PlayerName()
                {
                    Name = playersListDialog.PlayerName,
                };
                PlayerNames.Add(playerName);
            }
            catch (Exception e)
            {
                ErrorIO.AddError(e);
            }
        }

        [ListInput("キャラグラフィックデータ", additionButtonEventMethodName: "AddCharacterImageAsync")]
        public UndoRedoCollection<CharacterImage> CharacterImages { get; set; }

        public async Task AddCharacterImageAsync()
        {
            var file = RequireOpenFileDialog();
            if (file == "" || !asd.Engine.File.Exists(file)) return;
            CharacterImage item = await CharacterImage.LoadCharacterImageAsync(file);
            CharacterImages.Add(item);
        }

        public event Func<string> RequireOpenFileDialog = delegate { return null; };

        public event Func<MapEventIO.ActorIO, Task<IActor>> SearchActor = delegate { return null; };

        [ListInput("シナリオ", "SelectedComponent")]
        public UndoRedoCollection<MapEventComponent> EventComponents { get; set; }

        public MapEventComponent SelectedComponent
        {
            get => _selectedComponent;
            set
            {
                _selectedComponent = value;
                Simulate(value);
            }
        }

        void Simulate(MapEventComponent end)
        {
            foreach (var item in Actors)
            {
                item.Position = item.InitPosition;
                item.Active = true;
                item.IsSimulateEvent = true;
            }
            MainCamera.Position = MainCamera.InitPosition;
            MainCamera.Active = true;

            foreach (var item in Actors)
            {
                item.ClearTexture();
                item.SetTexture(item.Position, new asd.Color(100, 255, 100));
            }
            MainCamera.ClearGeometry();
            MainCamera.SetGeometry(Layer, MainCamera.Position, new asd.Color(255, 0, 0, 100));

            foreach (var component in EventComponents)
            {
                var tempPos = new Dictionary<Actor, asd.Vector2DF>();
                var cameraPos = MainCamera.Position;
                foreach (var item in Actors)
                {
                    item.ClearTexture();
                    item.SetTexture(item.Position, new asd.Color(100, 255, 100));
                    tempPos[item] = item.Position;
                }
                MainCamera.ClearGeometry();
                MainCamera.SetGeometry(Layer, MainCamera.Position, new asd.Color(255, 0, 0, 100));

                if (component is MoveComponent)
                {
                    UpdateCollision();
                    for (int i = 0; i < ((MoveComponent)component).Frame; i++)
                    {
                        foreach (var item in Actors)
                        {
                            if (!((MoveComponent)component).Commands.ContainsKey(item) ||
                                ((MoveComponent)component).Commands[item].MoveCommandElements.Count <= i) continue;
                            item.AddRequest(((MoveComponent)component).Commands[item].MoveCommandElements[i]);
                            item.Update();
                        }
                        if (((MoveComponent)component).CameraCommand.MoveCommandElements.Count > i)
                            MainCamera.AddRequest(((MoveComponent)component).CameraCommand.MoveCommandElements[i]);
                        MainCamera.Update();
                        if (Layer is MapLayer map)
                            map.PhysicalWorld.Update();
                    }

                    for (int i = 0; i < 30; i++)
                    {
                        if (Layer is MapLayer map)
                            map.PhysicalWorld.Update();
                    }

                    foreach (var item in Actors)
                    {
                        item.SetTexture(item.Position, new asd.Color(255, 100, 100));
                    }
                    MainCamera.SetGeometry(Layer, MainCamera.Position, new asd.Color(255, 255, 0, 100));
                }
                if (component == end)
                {
                    foreach (var item in Actors)
                    {
                        item.Position = tempPos[item];
                    }
                    MainCamera.Position = cameraPos;
                    break;
                }
            }

            foreach (var item in Actors)
            {
                item.Active = false;
                item.IsSimulateEvent = false;
            }
            MainCamera.Active = false;
        }

        [Button("キャラを動かす")]
        public void AddMoveComponent()
        {
            EventComponents.Add(new MoveComponent(Actors, MainCamera));
        }

        [Button("キャラをしゃべらす")]
        public void AddTalkComponent()
        {
            EventComponents.Add(new TalkComponent(CharacterImages));
        }

        [FileInput("終了時遷移先マップ", "Binary Map File|*.map|All File|*.*")]
        public string ToMapPath
        {
            get => _toMapPath;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _toMapPath = value;
                OnPropertyChanged();
            }
        }

        [ListInput("終了時プレイアブルキャラ", additionButtonEventMethodName: "AddPlayerData")]
        public UndoRedoCollection<PlayerName> PlayerNames { get; set; }

        [BoolInput("ドアIDを遷移に用いるか")]
        public bool IsUseDoorID
        {
            get => _isUseDoorID;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _isUseDoorID = value;
                OnPropertyChanged();
            }
        }

        [VectorInput("遷移先座標")]
        public asd.Vector2DF MoveToPosition
        {
            get => _moveToPosition;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _moveToPosition = value;
                OnPropertyChanged();
            }
        }

        [NumberInput("遷移先ドアID")]
        public int DoorID
        {
            get => _doorID;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _doorID = value;
                OnPropertyChanged();
            }
        }

        public void AddPlayerData()
        {
            PlayersListDialog playersListDialog = new PlayersListDialog();
            if (playersListDialog.Show() != PlayersListDialogResult.OK) return;

            var playerName = new PlayerName()
            {
                Name = playersListDialog.PlayerName,
            };
            PlayerNames.Add(playerName);
        }

        public asd.RectF Rect
        {
            get => Shape.DrawingArea;
            set
            {
                Position = value.Position;
                Size = value.Size;
            }
        }

        public asd.Vector2DF BottomRight => Position + Size;

        asd.RectF rect;
        private string _toMapPath;
        private asd.Vector2DF _moveToPosition;
        private int _doorID;
        private bool _isUseDoorID;

        public void StartMove()
        {
            rect = Rect;
        }

        public void EndMove()
        {
            UndoRedoManager.ChangeProperty(this, Rect, rect, "Rect");
        }

        public MapEvent()
        {
            CameraGroup = 1;
            Shape = new asd.RectangleShape();
            Color = new asd.Color(0, 255, 0, 100);
            DrawingPriority = 2;

            EventComponents = new UndoRedoCollection<MapEventComponent>();
            Actors = new UndoRedoCollection<Actor>();
            Actors.CollectionChanged += Actors_CollectionChanged;
            CharacterImages = new UndoRedoCollection<CharacterImage>();
            PlayerNames = new UndoRedoCollection<PlayerName>();
            MainCamera = new Camera();
        }

        public void SetInitCameraPosition()
        {
            MainCamera.InitPosition = Shape.DrawingArea.Position + Shape.DrawingArea.Size / 2.0f - new asd.Vector2DF(400, 300);
        }

        void Actors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var actor in e.NewItems?.OfType<Actor>() ?? e.OldItems?.OfType<Actor>())
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        actor.MapEvent = this;
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        actor.ClearTexture();
                        actor.MapEvent = null;
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        break;
                    default:
                        break;
                }
            }
        }

        public static async Task<MapEvent> CreateMapEventAsync(MapEventIO mapEventIO, Func<MapEventIO.ActorIO, Task<IActor>> searchFunc)
        {
            var mapEvent = new MapEvent();
            mapEvent.SearchActor += searchFunc;
            mapEvent.Position = mapEventIO.Position;
            mapEvent.Size = mapEventIO.Size;
            mapEvent.MainCamera.InitPosition = mapEventIO.Camera != null && mapEventIO.Camera.InitPosition != null ?
                (asd.Vector2DF)mapEventIO.Camera.InitPosition : new asd.Vector2DF();
            mapEvent.ToMapPath = mapEventIO.ToMapPath;
            mapEvent.MoveToPosition = mapEventIO.MoveToPosition;
            mapEvent.DoorID = mapEventIO.DoorID;
            mapEvent.IsUseDoorID = mapEventIO.IsUseDoorID;
            mapEvent.ID = mapEventIO.ID;
            if (mapEventIO.PlayerNames != null)
            {
                foreach (var item in mapEventIO.PlayerNames)
                {
                    mapEvent.PlayerNames.Add(new PlayerName() { Name = item });
                }
            }
            foreach (var item in mapEventIO.Actors)
            {
                try
                {
                    var actor = new Actor(await mapEvent.SearchActor(item));
                    actor.InitPosition = item.InitPosition;
                    mapEvent.Actors.Add(actor);
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
            }
            foreach (var item in mapEventIO.CharacterImagePaths)
            {
                mapEvent.CharacterImages.Add(await CharacterImage.LoadCharacterImageAsync(item));
            }
            foreach (var item in mapEventIO.Components)
            {
                if (item is MoveComponentIO)
                {
                    var component = MoveComponent.LoadMoveComponent((MoveComponentIO)item, mapEvent.Actors, mapEvent.MainCamera);
                    mapEvent.EventComponents.Add(component);
                }
                if (item is TalkComponentIO)
                {
                    var component = TalkComponent.LoadTalkComponent((TalkComponentIO)item, mapEvent.CharacterImages);
                    mapEvent.EventComponents.Add(component);
                }
            }
            return mapEvent;
        }

        protected override void OnUpdate()
        {
            if (SelectedComponent != null) SelectedComponent.Update();
            base.OnUpdate();
        }

        public void OnSelected()
        {
            Layer.AddObject(MainCamera);
            MainCamera.SetGeometry(Layer, MainCamera.InitPosition, new asd.Color(255, 255, 0, 100));
            foreach (var item in Actors.Select(obj => obj.ToObject2D()).OfType<EventObject>())
            {
                item.IsActivePhysic = true;
            }
            Simulate(SelectedComponent);
        }

        public void OnUnselected()
        {
            foreach (var item in Actors)
            {
                item.ClearTexture();
            }
            MainCamera.ClearGeometry();
            MainCamera.Layer.RemoveObject(MainCamera);
            foreach (var item in Actors.Select(obj => obj.ToObject2D()).OfType<EventObject>())
            {
                Layer.AddObject(item);
                item.Position = item.StartPosition;
            }
        }

        public bool GetIsActive()
        {
            if (Actors.Any(obj => obj.Active)) return true;
            return MainCamera.Active;
        }

        public void UpdateCollision()
        {
            if (Layer is MapLayer map)
            {
                //初期化
                foreach (var item in Actors.Select(obj => obj.ToObject2D()).OfType<EventObject>())
                {
                    item.Collision = new Collision();
                    foreach (var item2 in item.Sensors)
                    {
                        if (item2.Value is SatPlayer.Game.Object.MapObject.Sensor sensor)
                        {
                            sensor.Update();
                            sensor.Collision = new Collision();
                        }
                    }
                }

                //MapObject,EventObject=>Obstacle
                foreach (var item in Actors.OfType<EventObject>())
                {
                    if (item.Collision is Collision mapObjectCollision) mapObjectCollision.IsCollidedWithObstacle = map.Obstacles.Any(obj => obj.GetIsCollidedWith(item.CollisionShape));
                }

                //Sensor=>All
                foreach (var item in Actors.Select(obj => obj.ToObject2D()).OfType<EventObject>().SelectMany(obj => obj.Sensors).Select(obj => obj.Value).OfType<SatPlayer.Game.Object.MapObject.Sensor>())
                {
                    if (item.Collision is Collision collision)
                    {
                        collision.IsCollidedWithObstacle = map.Obstacles.Any(obj => item.GetIsCollidedWith(obj));
                    }
                }
            }
        }

        [Button("消去")]
        public void Remove()
        {
            UndoRedoManager.ChangeObject2D(Layer, this, false);
            Layer.RemoveObject(this);
        }

        public MapEventIO ToIO()
        {
            MapEventIO mapEventIO = new MapEventIO()
            {
                Position = Position,
                Size = Size,
                Actors = Actors.Select(obj => (MapEventIO.ActorIO)obj).ToList(),
                Components = EventComponents.Select<MapEventComponent, MapEventComponentIO>(obj =>
                     {
                         if (obj is MoveComponent) return (MoveComponentIO)(MoveComponent)obj;
                         else if (obj is TalkComponent) return (TalkComponentIO)(TalkComponent)obj;
                         return null;
                     }).ToList(),
                CharacterImagePaths = CharacterImages.Select(obj => obj.Path).ToList(),
                Camera = new MapEventIO.CameraIO()
                {
                    InitPosition = MainCamera.InitPosition,
                },
                ToMapPath = ToMapPath,
                PlayerNames = PlayerNames.Select(obj => obj.Name).ToList(),
                MoveToPosition = MoveToPosition,
                DoorID = DoorID,
                IsUseDoorID = IsUseDoorID,
                ID = ID,
            };
            return mapEventIO;
        }

        public class Actor : IListInput
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private bool _active;
            private asd.Vector2DF _initPosition;
            private IActor _actorImp;

            internal asd.Object2D ToObject2D()
                => _actorImp as asd.Object2D;

            public int ID => _actorImp.ID;

            public string Path => _actorImp.Path ?? ID.ToString();
            public string Name => Path;

            [VectorInput("初期座標")]
            public asd.Vector2DF InitPosition
            {
                get => _initPosition;
                set
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    _initPosition = value;
                    OnPropertyChanged();
                }
            }

            public MapEvent MapEvent { get; internal set; }

            List<asd.TextureObject2D> TextureObjects { get; set; }

            public Actor(IActor originActor)
            {
                _actorImp = originActor;
                TextureObjects = new List<asd.TextureObject2D>();
                GroundShape = new asd.RectangleShape();
            }

            public void SetTexture(asd.Vector2DF position, asd.Color color)
            {
                var textureObject = new asd.TextureObject2D();
                textureObject.Position = position;
                textureObject.Color = color;
                textureObject.Texture = Texture;
                textureObject.CenterPosition = Texture.Size.To2DF() / 2;
                textureObject.DrawingPriority = 3;
                textureObject.CameraGroup = 1;
                TextureObjects.Add(textureObject);
                MapEvent?.Layer?.AddObject(textureObject);
            }

            public void ClearTexture()
            {
                foreach (var item in TextureObjects)
                {
                    item.Dispose();
                }
                TextureObjects.Clear();
            }

            public bool Active
            {
                get => _active;
                set
                {
                    _active = value;
                    if (_active)
                    {
                        foreach (var item in TextureObjects)
                        {
                            item.IsDrawn = false;
                        }
                        if (MapEvent?.Layer is MapLayer mapLayer)
                            _actorImp.SetCollision(mapLayer);
                        if (ToObject2D()?.Layer == null)
                            MapEvent?.Layer?.AddObject(ToObject2D());
                    }
                    else
                    {
                        foreach (var item in TextureObjects)
                        {
                            item.IsDrawn = true;
                        }
                        Position = InitPosition;
                        ToObject2D()?.Layer?.RemoveObject(ToObject2D());
                    }
                }
            }

            public bool IsSimulateEvent
            {
                get => _actorImp.IsEvent;
                set => _actorImp.IsEvent = value;
            }

            public bool IsCollidedWithGround
            {
                get
                {
                    if (Layer == null) return false;
                    return Layer.Objects.Any(obj =>
                    {
                        if (obj is CollisionBox collisionBox) return collisionBox.Shape.GetIsCollidedWith(GroundShape);
                        else if (obj is CollisionTriangle collisionTriangle) return collisionTriangle.Shape.GetIsCollidedWith(GroundShape);
                        return false;
                    });
                }
            }

            public void Update()
            {
                _actorImp.OnUpdate();
            }

            public asd.RectangleShape GroundShape { get; private set; }
            public asd.Texture2D Texture => _actorImp.Texture;

            public asd.Vector2DF Position { get => _actorImp.Position; set => _actorImp.Position = value; }
            public asd.Layer2D Layer => _actorImp.Layer;


            public void AddRequest(Dictionary<Inputs, bool> input)
            {
                _actorImp.MoveCommands.Enqueue(input);
            }

            public static explicit operator MapEventIO.ActorIO(Actor actor)
            {
                var actorIO = new MapEventIO.ActorIO()
                {
                    Path = actor._actorImp.Path,
                    ID = actor.ID,
                    InitPosition = actor.InitPosition,
                };
                return actorIO;
            }
        }

        public class Camera : asd.GeometryObject2D, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private asd.Vector2DF _position;
            private bool _active;

            public new asd.Vector2DF Position
            {
                get => _position;
                set
                {
                    _position = value;
                    Shape.DrawingArea = new asd.RectF(value, new asd.Vector2DF(800, 600));
                }
            }

            public asd.Vector2DF TargetPosition;
            private asd.Vector2DF _initPosition;

            public new asd.RectangleShape Shape
            {
                get => (asd.RectangleShape)base.Shape;
                set => base.Shape = value;
            }

            [VectorInput("初期座標")]
            public asd.Vector2DF InitPosition
            {
                get => _initPosition;
                set
                {
                    foreach (var item in GeometryObjects)
                    {
                        item.Position += value - _initPosition;
                    }
                    UndoRedoManager.ChangeProperty(this, value);
                    _initPosition = value;
                    OnPropertyChanged();
                }
            }

            List<asd.GeometryObject2D> GeometryObjects { get; set; }

            public Camera()
            {
                Shape = new asd.RectangleShape();
                TargetPosition = new asd.Vector2DF();
                GeometryObjects = new List<asd.GeometryObject2D>();
                Color = new asd.Color(255, 255, 0, 100);
                DrawingPriority = 2;
                CameraGroup = 1;
                IsDrawn = false;
                IsUpdated = false;
            }

            public void SetGeometry(asd.Layer2D layer, asd.Vector2DF position, asd.Color color)
            {
                var geometryObject = new asd.GeometryObject2D();
                geometryObject.Color = color;
                geometryObject.DrawingPriority = 3;
                geometryObject.CameraGroup = 1;
                geometryObject.Shape = new asd.RectangleShape()
                {
                    DrawingArea = new asd.RectF(position, ScalingLayer2D.OriginDisplaySize),
                };
                GeometryObjects.Add(geometryObject);
                layer.AddObject(geometryObject);
            }

            public void ClearGeometry()
            {
                foreach (var item in GeometryObjects)
                {
                    item.Dispose();
                }
                GeometryObjects.Clear();
            }

            public bool Active
            {
                get => _active;
                set
                {
                    _active = value;
                    if (_active)
                    {
                        foreach (var item in GeometryObjects)
                        {
                            item.IsDrawn = false;
                        }
                        IsDrawn = true;
                        IsUpdated = true;
                        TargetPosition = Position;
                    }
                    else
                    {
                        foreach (var item in GeometryObjects)
                        {
                            item.IsDrawn = true;
                        }
                        IsDrawn = false;
                        IsUpdated = false;
                    }
                }
            }

            public void Update()
            {
                OnUpdate();
            }

            protected override void OnUpdate()
            {
                InputCamera();

                asd.Vector2DF velocity = new asd.Vector2DF();

                velocity.X = GetVelocity((TargetPosition - Position).X);
                velocity.Y = GetVelocity((TargetPosition - Position).Y);

                Position += velocity;
                base.OnUpdate();
            }

            void InputCamera()
            {
                if (InputRequest == null)
                {
                    if (Input.GetInputState(Inputs.B) <= 0)
                    {
                        if (Input.GetInputState(Inputs.Up) > 0) TargetPosition.Y -= 100f / 60;
                        if (Input.GetInputState(Inputs.Down) > 0) TargetPosition.Y += 100f / 60;
                        if (Input.GetInputState(Inputs.Left) > 0) TargetPosition.X -= 100f / 60;
                        if (Input.GetInputState(Inputs.Right) > 0) TargetPosition.X += 100f / 60;
                    }
                    if (Input.GetInputState(Inputs.B) > 0)
                    {
                        if (Input.GetInputState(Inputs.Up) > 0) TargetPosition.Y -= 200f / 60;
                        if (Input.GetInputState(Inputs.Down) > 0) TargetPosition.Y += 200f / 60;
                        if (Input.GetInputState(Inputs.Left) > 0) TargetPosition.X -= 200f / 60;
                        if (Input.GetInputState(Inputs.Right) > 0) TargetPosition.X += 200f / 60;
                    }
                }
                else
                {
                    if (!GetInputRequestState(Inputs.B))
                    {
                        if (GetInputRequestState(Inputs.Up)) TargetPosition.Y -= 100f / 60;
                        if (GetInputRequestState(Inputs.Down)) TargetPosition.Y += 100f / 60;
                        if (GetInputRequestState(Inputs.Left)) TargetPosition.X -= 100f / 60;
                        if (GetInputRequestState(Inputs.Right)) TargetPosition.X += 100f / 60;
                    }
                    else
                    {
                        if (GetInputRequestState(Inputs.Up)) TargetPosition.Y -= 200f / 60;
                        if (GetInputRequestState(Inputs.Down)) TargetPosition.Y += 200f / 60;
                        if (GetInputRequestState(Inputs.Left)) TargetPosition.X -= 200f / 60;
                        if (GetInputRequestState(Inputs.Right)) TargetPosition.X += 200f / 60;
                    }
                    InputRequest = null;
                }
            }

            public Dictionary<Inputs, bool> InputRequest { get; private set; }

            public void AddRequest(Dictionary<Inputs, bool> input)
            {
                InputRequest = new Dictionary<Inputs, bool>(input);
            }

            public bool GetInputRequestState(Inputs inputs)
            {
                if (InputRequest == null || !InputRequest.ContainsKey(inputs)) return false;
                return InputRequest[inputs];
            }

            float GetVelocity(float distance)
            {
                if (Math.Abs(distance) < 1.5f) return 0;
                return Math.Abs(distance * 0.07f) > 1.0f ? distance * 0.07f : Math.Sign(distance) * 1.0f;
            }
        }

        public class PlayerName : IListInput
        {
            public string Name { get; set; }
        }
    }
}
