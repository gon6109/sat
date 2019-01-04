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

namespace SatCore.MapEditor.MapEvent
{
    public class MapEvent : asd.GeometryObject2D, INotifyPropertyChanged, IMovable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        PhysicalWorld refWorld;
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
            if (((MainMapLayer2D)Layer).CurrentToolType != ToolType.Select ||
                ((MainMapLayer2D)Layer).Objects.Count(obj => obj is NPCMapObject) <= Actors.Count(obj => !obj.IsUseName))
                return;
            ((MainMapLayer2D)Layer).CurrentToolType = ToolType.SelectNPC;
        }

        public void AddNPCActor(string characterDataPath, int id, asd.Vector2DF initPosition)
        {
            var actor = new Actor(characterDataPath, refWorld)
            {
                ID = id,
                InitPosition = initPosition,
                Position = initPosition,
                IsUseName = false,
            };
            Actors.Add(actor);
            Layer.AddObject(actor);
            actor.SetTexture(Layer, actor.InitPosition, new asd.Color(255, 255, 255));
        }

        [Button("プレイヤーを登録")]
        public void AddPlayerActor()
        {
            if (PlayersListDialog.GetPlayersData().Count <= Actors.Count(obj => obj.IsUseName)) return;

            PlayersListDialog playersListDialog = new PlayersListDialog();
            if (playersListDialog.Show() != PlayersListDialogResult.OK) return;

            var playerData = SatIO.PlayerIO.GetPlayerIO(playersListDialog.FileName);
            if (Actors.Any(obj => obj.Name == playerData.Name)) return;
            var actor = new Actor(playersListDialog.FileName, refWorld)
            {
                Name = playerData.Name,
                IsUseName = true,
            };
            actor.InitPosition = Position + new asd.Vector2DF(Size.X, 0) - actor.Texture.Size.To2DF();
            actor.Position = actor.InitPosition;
            for (int i = 0; i < 60; i++)
            {
                refWorld.Update();
            }
            actor.InitPosition = actor.CollisionShape.DrawingArea.Position + actor.CollisionShape.CenterPosition;
            Actors.Add(actor);
            Layer.AddObject(actor);
            actor.SetTexture(Layer, actor.InitPosition, new asd.Color(255, 255, 255));

            var playerName = new PlayerName()
            {
                Name = playerData.Name,
            };
            PlayerNames.Add(playerName);
        }

        [ListInput("キャラグラフィックデータ", additionButtonEventMethodName: "AddCharacterImage")]
        public UndoRedoCollection<CharacterImage> CharacterImages { get; set; }

        public void AddCharacterImage()
        {
            var file = RequireOpenFileDialog();
            if (file == "" || !asd.Engine.File.Exists(file)) return;
            CharacterImages.Add(CharacterImage.LoadCharacterImage(file));
        }

        public Func<string> RequireOpenFileDialog { get; set; }

        public Func<MapEventIO.ActorIO, string> SearchCharacterDataPath { get; set; }

        [ListInput("シナリオ", "SelectedComponent")]
        public UndoRedoCollection<MapEventComponent> EventComponents { get; set; }

        public MapEventComponent SelectedComponent
        {
            get => _selectedComponent;
            set
            {
                _selectedComponent = value;
                Simulation(value);
            }
        }

        void Simulation(MapEventComponent end)
        {
            foreach (var item in Actors)
            {
                item.Position = item.InitPosition;
                item.Active = true;
            }
            MainCamera.Position = MainCamera.InitPosition;
            MainCamera.Active = true;

            foreach (var component in EventComponents)
            {
                var tempPos = new Dictionary<Actor, asd.Vector2DF>();
                var cameraPos = MainCamera.Position;
                foreach (var item in Actors)
                {
                    item.ClearTexture();
                    item.SetTexture(Layer, item.Position, new asd.Color(100, 255, 100));
                    tempPos[item] = item.Position;
                }
                MainCamera.ClearGeometry();
                MainCamera.SetGeometry(Layer, MainCamera.Position, new asd.Color(255, 0, 0, 100));

                if (component is MoveComponent)
                {
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
                        refWorld.Update();
                    }

                    for (int i = 0; i < 30; i++)
                    {
                        refWorld.Update();
                    }

                    foreach (var item in Actors)
                    {
                        item.SetTexture(Layer, item.Position, new asd.Color(255, 100, 100));
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

        [FileInput("終了時遷移先マップ", "Binary Map File|*.bmap|All File|*.*")]
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

            var playerData = SatIO.PlayerIO.GetPlayerIO(playersListDialog.FileName);
            if (PlayerNames.Any(obj => obj.Name == playerData.Name)) return;
            var playerName = new PlayerName()
            {
                Name = playerData.Name,
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

        public MapEvent(Func<string> requireOpenFileDialogFunc,
            Func<MapEventIO.ActorIO, string> searchCharacterDataPathFunc, PhysicalWorld world)
        {
            CameraGroup = 1;
            Shape = new asd.RectangleShape();
            Color = new asd.Color(0, 255, 0, 100);
            DrawingPriority = 2;

            EventComponents = new UndoRedoCollection<MapEventComponent>();
            RequireOpenFileDialog = requireOpenFileDialogFunc;
            SearchCharacterDataPath = searchCharacterDataPathFunc;
            Actors = new UndoRedoCollection<Actor>();
            Actors.CollectionChanged += Actors_CollectionChanged;
            CharacterImages = new UndoRedoCollection<CharacterImage>();
            PlayerNames = new UndoRedoCollection<PlayerName>();
            MainCamera = new Camera();
            refWorld = world;
        }

        public void SetInitCameraPosition()
        {
            MainCamera.InitPosition = Shape.DrawingArea.Position + Shape.DrawingArea.Size / 2.0f - new asd.Vector2DF(400, 300);
        }

        void Actors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.OldItems[0] as Actor == null) return;
            var actor = e.OldItems[0] as Actor;
            actor.ClearTexture();
            actor.Dispose();
        }

        public MapEvent(MapEventIO mapEventIO, Func<string> requireOpenFileDialogFunc,
            Func<MapEventIO.ActorIO, string> searchCharacterDataPathFunc, PhysicalWorld world)
        {
            Shape = new asd.RectangleShape();
            Color = new asd.Color(0, 255, 0, 100);
            DrawingPriority = 2;
            CameraGroup = 1;

            EventComponents = new UndoRedoCollection<MapEventComponent>();
            RequireOpenFileDialog = requireOpenFileDialogFunc;
            SearchCharacterDataPath = searchCharacterDataPathFunc;
            Actors = new UndoRedoCollection<Actor>();
            Actors.CollectionChanged += Actors_CollectionChanged;
            CharacterImages = new UndoRedoCollection<CharacterImage>();

            PlayerNames = new UndoRedoCollection<PlayerName>();
            MainCamera = new Camera();
            refWorld = world;

            LoadMapEventIO(mapEventIO, world);
        }

        void LoadMapEventIO(MapEventIO mapEventIO, PhysicalWorld world)
        {
            Position = mapEventIO.Position;
            Size = mapEventIO.Size;
            MainCamera.InitPosition = (mapEventIO.Camera != null && mapEventIO.Camera.InitPosition != null) ?
                (asd.Vector2DF)mapEventIO.Camera.InitPosition : new asd.Vector2DF();
            ToMapPath = mapEventIO.ToMapPath;
            MoveToPosition = mapEventIO.MoveToPosition;
            DoorID = mapEventIO.DoorID;
            IsUseDoorID = mapEventIO.IsUseDoorID;
            ID = mapEventIO.ID;
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
                    var actor = new Actor(SearchCharacterDataPath(item), world);
                    actor.Name = item.Name;
                    actor.ID = item.ID;
                    actor.IsUseName = item.IsUseName;
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

        protected override void OnUpdate()
        {
            if (SelectedComponent != null) SelectedComponent.Update();
            base.OnUpdate();
        }

        public void OnSelected()
        {
            foreach (var item in Actors)
            {
                Layer.AddObject(item);
                item.CollisionShape.IsActive = true;
                item.SetTexture(Layer, item.InitPosition, new asd.Color(255, 255, 255));
            }
            Layer.AddObject(MainCamera);
            MainCamera.SetGeometry(Layer, MainCamera.InitPosition, new asd.Color(255, 255, 0, 100));
        }

        public void OnUnselected()
        {
            foreach (var item in Actors)
            {
                item.ClearTexture();
                item.CollisionShape.IsActive = false;
                item.Layer.RemoveObject(item);
            }
            MainCamera.ClearGeometry();
            MainCamera.Layer.RemoveObject(MainCamera);
        }

        public bool GetIsActive()
        {
            if (Actors.Any(obj => obj.Active)) return true;
            return MainCamera.Active;
        }

        [Button("消去")]
        public void Remove()
        {
            UndoRedoManager.ChangeObject2D(Layer, this, false);
            Layer.RemoveObject(this);
        }

        public static explicit operator MapEventIO(MapEvent mapEvent)
        {
            MapEventIO mapEventIO = new MapEventIO()
            {
                Position = mapEvent.Position,
                Size = mapEvent.Size,
                Actors = mapEvent.Actors.Select(obj => (MapEventIO.ActorIO)obj).ToList(),
                Components = mapEvent.EventComponents.Select<MapEventComponent, MapEventComponentIO>(obj =>
                     {
                         if (obj is MoveComponent) return (MoveComponentIO)(MoveComponent)obj;
                         else if (obj is TalkComponent) return (TalkComponentIO)(TalkComponent)obj;
                         return null;
                     }).ToList(),
                CharacterImagePaths = mapEvent.CharacterImages.Select(obj => obj.Path).ToList(),
                Camera = new MapEventIO.CameraIO()
                {
                    InitPosition = mapEvent.MainCamera.InitPosition,
                },
                ToMapPath = mapEvent.ToMapPath,
                PlayerNames = mapEvent.PlayerNames.Select(obj => obj.Name).ToList(),
                MoveToPosition = mapEvent.MoveToPosition,
                DoorID = mapEvent.DoorID,
                IsUseDoorID = mapEvent.IsUseDoorID,
                ID = mapEvent.ID,
            };
            return mapEventIO;
        }

        public class Actor : MotionEditor.Character, IListInput
        {
            private string _name;
            private bool _active;
            private asd.Vector2DF _initPosition;

            public int ID { get; set; }
            public string Name
            {
                get
                {
                    if (IsUseName) return _name;
                    else return ID.ToString();
                }
                set => _name = value;
            }
            public bool IsUseName { get; set; }

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

            List<asd.TextureObject2D> TextureObjects { get; set; }

            public Actor(string characterDataPath, PhysicalWorld world) : base(characterDataPath, world)
            {
                CameraGroup = 1;
                TextureObjects = new List<asd.TextureObject2D>();
                State = UprightLeftState;
                CollisionShape.DrawingArea = new asd.RectF(new asd.Vector2DF(), Texture.Size.To2DF());
                CollisionShape.GroupIndex = -1;
                IsDrawn = false;
                IsUpdated = false;
            }

            public void SetTexture(asd.Layer2D layer, asd.Vector2DF position, asd.Color color)
            {
                var textureObject = new asd.TextureObject2D();
                textureObject.Position = position;
                textureObject.Color = color;
                textureObject.Texture = Texture;
                textureObject.CenterPosition = Texture.Size.To2DF() / 2;
                textureObject.DrawingPriority = 3;
                textureObject.CameraGroup = 1;
                TextureObjects.Add(textureObject);
                layer.AddObject(textureObject);
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
                        IsDrawn = true;
                        IsUpdated = true;
                    }
                    else
                    {
                        foreach (var item in TextureObjects)
                        {
                            item.IsDrawn = true;
                        }
                        IsDrawn = false;
                        IsUpdated = false;
                    }
                }
            }

            public override bool IsColligedWithGround
            {
                get
                {
                    if (Layer == null) return false;
                    return Layer.Objects.Any(obj =>
                    {
                        if (obj is CollisionBox) return ((CollisionBox)obj).Shape.GetIsCollidedWith(GroundShape);
                        else if (obj is CollisionTriangle) return ((CollisionTriangle)obj).Shape.GetIsCollidedWith(GroundShape);
                        return false;
                    });
                }
            }

            public void Update()
            {
                OnUpdate();
            }

            protected override void InputPlayer()
            {
                if (InputRequest == null)
                {
                    base.InputPlayer();
                    return;
                }

                if (!State.Contains("jump") && IsColligedWithGround)
                {
                    if (GetInputRequestState(Inputs.Up))
                    {
                        State = State.Contains("_l") ? JumpLeftState : JumpRightState;
                        InputRequest.Clear();
                        return;
                    }
                    if (GetInputRequestState(Inputs.Left) && !GetInputRequestState(Inputs.B)) State = WalkLeftState;
                    if (GetInputRequestState(Inputs.Right) && !GetInputRequestState(Inputs.B)) State = WalkRightState;
                    if (GetInputRequestState(Inputs.Left) && GetInputRequestState(Inputs.B)) State = DashLeftState;
                    if (GetInputRequestState(Inputs.Right) && GetInputRequestState(Inputs.B)) State = DashRightState;
                    if ((GetInputRequestState(Inputs.Right) && GetInputRequestState(Inputs.Left))
                        || (!GetInputRequestState(Inputs.Right) && !GetInputRequestState(Inputs.Left)))
                    {
                        State = State.Contains("_l") ? UprightLeftState : UprightRightState;
                    }
                }
                else
                {
                    if (GetInputRequestState(Inputs.Left) && !GetInputRequestState(Inputs.B)) State = CollisionShape.Velocity.Y < 0 ? UpperLeftState : LowerLeftState;
                    if (GetInputRequestState(Inputs.Right) && !GetInputRequestState(Inputs.B)) State = CollisionShape.Velocity.Y < 0 ? UpperRightState : LowerRightState;
                    if (GetInputRequestState(Inputs.Left) && GetInputRequestState(Inputs.B)) State = CollisionShape.Velocity.Y < 0 ? DashUpperLeftState : DashLowerLeftState;
                    if (GetInputRequestState(Inputs.Right) && GetInputRequestState(Inputs.B)) State = CollisionShape.Velocity.Y < 0 ? DashUpperRightState : DashLowerRightState;
                    if ((GetInputRequestState(Inputs.Right) && GetInputRequestState(Inputs.Left))
                        || (!GetInputRequestState(Inputs.Right) && !GetInputRequestState(Inputs.Left)))
                    {
                        if (CollisionShape.Velocity.Y < 0) State = State.Contains("_l") ? UpLeftState : UpRightState;
                        else State = State.Contains("_l") ? DownLeftState : DownRightState;
                    }
                }
                InputRequest = null;
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

            public static explicit operator MapEventIO.ActorIO(Actor actor)
            {
                var actorIO = new MapEventIO.ActorIO()
                {
                    Name = actor.Name,
                    ID = actor.ID,
                    IsUseName = actor.IsUseName,
                    InitPosition = actor.InitPosition,
                };
                return actorIO;
            }
        }

        public class Camera : asd.GeometryObject2D, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
                    DrawingArea = new asd.RectF(position, BaseComponent.Base.ScreenSize.To2DF()),
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
