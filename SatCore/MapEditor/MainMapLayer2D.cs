﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SatIO;
using PhysicAltseed;
using SatIO.MapEventIO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BaseComponent;

namespace SatCore.MapEditor
{
    public delegate void EventDelegate();

    /// <summary>
    /// マップのメインレイヤー
    /// </summary>
    public class MainMapLayer2D : asd.Layer2D, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 選択オブジェクトが変更されたとき
        /// </summary>
        public EventDelegate OnChangeSelectedObject { get; set; }

        /// <summary>
        /// ドアが作成されたとき
        /// </summary>
        public EventDelegate OnCreateDoor { get; set; }

        /// <summary>
        /// マップオブジェクトが作成されたとき
        /// </summary>
        public EventDelegate OnCreateMapObject { get; set; }

        /// <summary>
        /// エディターパネルにフォーカスさせる
        /// </summary>
        public Action FocusToEditorPanel { get; set; }
        void DefaultFunc() { }

        /// <summary>
        /// ファイルを開くダイアログをUI側に要求する
        /// </summary>
        public Func<string> RequireOpenFileDialog { get; set; }
        string DefalutStringFunc() { return ""; }

        /// <summary>
        /// メインのカメラ
        /// </summary>
        public asd.CameraObject2D ScrollCamera { get; private set; }
        public float Zoom { get; set; }

        [VectorInput("マップサイズ")]
        public asd.Vector2DF WorldSize { get; set; }

        /// <summary>
        /// 選択されているオブジェクト
        /// </summary>
        public asd.Object2D SelectedObject { get; private set; }

        asd.CircleShape cursorShape;
        asd.Vector2DF preMousePosition;
        object dragObject;

        List<CollisionBox> CollisionBoxes { get => Objects.OfType<CollisionBox>().ToList(); }
        List<CollisionTriangle> CollisionTriangles { get => Objects.OfType<CollisionTriangle>().ToList(); }
        List<Door> Doors { get => Objects.OfType<Door>().ToList(); }
        List<MapObject> MapObjects { get => Objects.Where(obj => obj is MapObject && !(obj is EventObject)).Cast<MapObject>().ToList(); }
        List<EventObject> EventObjects { get => Objects.OfType<EventObject>().ToList(); }
        List<MapEvent.MapEvent> MapEvents { get => Objects.OfType<MapEvent.MapEvent>().ToList(); }
        List<CameraRestriction> CameraRestrictions { get => Objects.OfType<CameraRestriction>().ToList(); }
        List<SavePoint> SavePoints { get => Objects.OfType<SavePoint>().ToList(); }

        ToolType currentToolType;
        /// <summary>
        /// 現在のツールタイプ
        /// </summary>
        public ToolType CurrentToolType
        {
            get => currentToolType;
            set
            {
                if (currentToolType == value) return;

                if (value == ToolType.SelectEventObject)
                {
                    currentToolType = value;
                    OnChangeSelectEventObject(false);
                    return;
                }
                if (currentToolType == ToolType.SelectEventObject)
                {
                    currentToolType = value;
                    OnChangeSelectEventObject(true);
                    return;
                }

                if (SelectedObject is MapEvent.MapEvent && value != ToolType.Select) ((MapEvent.MapEvent)SelectedObject).OnUnselected();

                foreach (var item in dotObjects)
                {
                    item.Dispose();
                };
                dotObjects.Clear();
                polygonObject.Dispose();
                SelectedObject = null;
                currentToolType = value;
            }
        }

        void OnChangeSelectEventObject(bool isDrawn)
        {
            foreach (var item in CollisionBoxes)
            {
                item.IsDrawn = isDrawn;
            }
            foreach (var item in CollisionTriangles)
            {
                item.IsDrawn = isDrawn;
            }
            foreach (var item in MapObjects)
            {
                item.IsDrawn = isDrawn;
            }
            foreach (var item in Doors)
            {
                item.IsDrawn = isDrawn;
            }
        }

        /// <summary>
        /// 頂点つまみオブジェクトコレクション
        /// </summary>
        List<asd.GeometryObject2D> dotObjects;

        /// <summary>
        /// 選択領域オブジェクト
        /// </summary>
        asd.GeometryObject2D polygonObject;

        PhysicalWorld physicalWorld;

        List<EventObject> eventObjects;

        public MainMapLayer2D()
        {
            OnChangeSelectedObject = DefaultFunc;
            OnCreateDoor = DefaultFunc;
            OnCreateMapObject = DefaultFunc;
            FocusToEditorPanel = DefaultFunc;
            RequireOpenFileDialog = DefalutStringFunc;
            ScrollCamera = new asd.CameraObject2D();
            ScrollCamera.CameraGroup = 1;
            cursorShape = new asd.CircleShape();
            ScrollCamera.Src = new asd.RectI(0, 0, asd.Engine.WindowSize.X, asd.Engine.WindowSize.Y);
            ScrollCamera.Dst = new asd.RectI(0, 0, asd.Engine.WindowSize.X, asd.Engine.WindowSize.Y);
            dotObjects = new List<asd.GeometryObject2D>();
            polygonObject = new asd.GeometryObject2D();
            polygonObject.CameraGroup = 1;
            physicalWorld = new PhysicalWorld(new asd.RectF(-200, -200, 20400, 5400), new asd.Vector2DF(0, 2000));
            Zoom = 1.0f;
        }

        /// <summary>
        /// マップデータを読み込む
        /// </summary>
        /// <param name="mapData">マップデータ</param>
        public void LoadMapData(MapIO mapData)
        {
            WorldSize = mapData.Size;

            if (mapData.MapObjects != null)
            {
                foreach (var item in mapData.MapObjects)
                {
                    var temp = new MapObject(item);
                    AddObject(temp);
                }
            }

            eventObjects = new List<EventObject>();
            if (mapData.EventObjects != null)
            {
                foreach (var item in mapData.EventObjects)
                {
                    var temp = new EventObject(item);
                    if (!eventObjects.Any(obj => obj.ID == temp.ID))
                    {
                        AddObject(temp);
                        eventObjects.Add(temp);
                    }
                }
            }

            if (mapData.MapEvents != null)
            {
                foreach (var item in mapData.MapEvents)
                {
                    var temp = new MapEvent.MapEvent(item, RequireOpenFileDialog, SearchCharacterDataPath, physicalWorld);
                    AddObject(temp);
                }
            }

            if (mapData.Doors != null)
            {
                foreach (var item in mapData.Doors)
                {
                    try
                    {
                        var temp = (Door)item;
                        AddObject(temp);
                    }
                    catch (Exception e)
                    {
                        ErrorIO.AddError(e);
                    }
                }
            }

            if (mapData.CollisionBoxes != null)
            {
                foreach (var item in mapData.CollisionBoxes)
                {
                    var temp = new CollisionBox(item, physicalWorld);
                    AddObject(temp);
                }
            }

            if (mapData.CollisionTriangles != null)
            {
                foreach (var item in mapData.CollisionTriangles)
                {
                    var temp = new CollisionTriangle(item, physicalWorld);
                    AddObject(temp);
                }
            }

            if (mapData.CameraRestrictions != null)
            {
                foreach (var item in mapData.CameraRestrictions)
                {
                    var temp = new CameraRestriction(item);
                    AddObject(temp);
                }
            }

            if (mapData.SavePoints != null)
            {
                foreach (var item in mapData.SavePoints)
                {
                    var temp = new SavePoint(item);
                    AddObject(temp);
                }
            }
        }

        string SearchCharacterDataPath(SatIO.MapEventIO.MapEventIO.ActorIO actorIO)
        {
            if (actorIO.IsUseName)
            {
                return PlayersListDialog.GetPlayersData().First(obj => obj.Value.Name == actorIO.Name).Key;
            }
            else
            {
                try
                {
                    if (EventObjects.Count != 0 || eventObjects.Count == 0) return EventObjects.First(obj => obj.ID == actorIO.ID).MotionPath;
                    else return eventObjects.First(obj => obj.ID == actorIO.ID).MotionPath;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// マップをセーブする
        /// </summary>
        /// <param name="mapData">保存先</param>
        public void SaveMapData(MapIO mapData)
        {
            mapData.Size = WorldSize;

            foreach (var item in CollisionBoxes)
            {
                var temp = new CollisionBoxIO()
                {
                    Position = ((asd.RectangleShape)item.Shape).DrawingArea.Position,
                    Size = ((asd.RectangleShape)item.Shape).DrawingArea.Size
                };
                mapData.CollisionBoxes.Add(temp);
            }

            foreach (var item in CollisionTriangles)
            {
                var temp = new CollisionTriangleIO();
                for (int i = 0; i < 3; i++) temp.vertexes[i] = item.Shape.GetPointByIndex(i);
                mapData.CollisionTriangles.Add(temp);
            }

            foreach (var item in MapObjects)
            {
                mapData.MapObjects.Add((MapObjectIO)item);
            }

            foreach (var item in EventObjects)
            {
                mapData.EventObjects.Add((EventObjectIO)item);
            }

            foreach (var item in MapEvents)
            {
                mapData.MapEvents.Add((MapEventIO)item);
            }

            foreach (var item in Doors)
            {
                mapData.Doors.Add((DoorIO)item);
            }

            foreach (var item in CameraRestrictions)
            {
                var temp = new CameraRestrictionIO()
                {
                    Position = ((asd.RectangleShape)item.Shape).DrawingArea.Position,
                    Size = ((asd.RectangleShape)item.Shape).DrawingArea.Size
                };
                mapData.CameraRestrictions.Add(temp);
            }

            foreach (var item in SavePoints)
            {
                var temp = new SavePointIO()
                {
                    Position = item.Position,
                    ID = item.ID,
                };
                mapData.SavePoints.Add(temp);
            }
        }

        public SelectType GetSelectedObjectType()
        {
            switch (SelectedObject)
            {
                case CollisionBox box:
                    return SelectType.Box;
                case CollisionTriangle triangle:
                    return SelectType.Triangle;
                case EventObject eventObject:
                    return SelectType.EventObject;
                case MapObject mapObject:
                    return SelectType.Object;
                case Door door:
                    return SelectType.Door;
                case MapEvent.MapEvent mapEvent:
                    return SelectType.Event;
                case CameraRestriction cameraRestriction:
                    return SelectType.CameraRestriction;
                case SavePoint savePoint:
                    return SelectType.SavePoint;
                default:
                    return SelectType.None;
            }
        }

        protected override void OnAdded()
        {
            cursorShape.Position = GetMouseRelativePosition();
            cursorShape.OuterDiameter = 3;
            AddObject(ScrollCamera);
            CurrentToolType = ToolType.Select;
            base.OnAdded();
        }

        protected override void OnUpdating()
        {
            base.OnUpdating();
            if (Mouse.MiddleButton == asd.ButtonState.Hold)
                ScrollCamera.Src = new asd.RectI(ScrollCamera.Src.Position - GetMouseMoveVector().To2DI(), ScrollCamera.Src.Size);
            ScrollCamera.Src = new asd.RectI(ScrollCamera.Src.Position, (asd.Engine.WindowSize.To2DF() * Zoom).To2DI());
            ScrollCamera.Dst = new asd.RectI(0, 0, asd.Engine.WindowSize.X, asd.Engine.WindowSize.Y);
            Zoom += Mouse.MouseWheel * 0.05f;
            cursorShape.Position = GetMouseRelativePosition();
            foreach (var item in dotObjects)
            {
                if (item.Shape.ShapeType == asd.ShapeType.CircleShape) ((asd.CircleShape)item.Shape).OuterDiameter = 8.0f * (Zoom > 1 ? Zoom : 1);
            }
        }

        protected override void OnUpdated()
        {

            switch (CurrentToolType)
            {
                case ToolType.Select:
                    if (!OperationSelectedObject())
                    {
                        SelectObject();
                        if (SelectedObject == null && Mouse.LeftButton == asd.ButtonState.Hold)
                            ScrollCamera.Src = new asd.RectI(ScrollCamera.Src.Position - GetMouseMoveVector().To2DI(), ScrollCamera.Src.Size);
                    }
                    break;
                case ToolType.Box:
                    SetCollisionBox();
                    break;
                case ToolType.Triangle:
                    SetCollisionTriangle();
                    break;
                case ToolType.Door:
                    SetDoor();
                    break;
                case ToolType.Object:
                    SetMapObject();
                    break;
                case ToolType.EventObject:
                    SetEventObject();
                    break;
                case ToolType.Event:
                    SetMapEvent();
                    break;
                case ToolType.SelectEventObject:
                    SelectEventObject();
                    break;
                case ToolType.CameraRestriction:
                    SetCameraRestriction();
                    break;
                case ToolType.SavePoint:
                    SetSavePoint();
                    break;
            }
            base.OnUpdated();

            preMousePosition = Mouse.Position;
        }

        void SetCollisionBox()
        {
            if (!polygonObject.IsAlive && Mouse.LeftButton == asd.ButtonState.Push)
            {
                polygonObject = new asd.GeometryObject2D();
                polygonObject.Shape = new asd.RectangleShape()
                {
                    DrawingArea = new asd.RectF(GetMouseRelativePosition(), new asd.Vector2DF())
                };
                polygonObject.CameraGroup = 1;
                polygonObject.Color = new asd.Color(0, 0, 255, 100);
                AddObject(polygonObject);
            }
            else if (polygonObject.IsAlive && Mouse.LeftButton == asd.ButtonState.Hold)
            {
                ((asd.RectangleShape)polygonObject.Shape).DrawingArea
                    = new asd.RectF(((asd.RectangleShape)polygonObject.Shape).DrawingArea.Position, ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size + GetMouseMoveVector());
            }
            else if (polygonObject.IsAlive && Mouse.LeftButton == asd.ButtonState.Release)
            {
                UndoRedoManager.Enable = false;
                CollisionBox collision = new CollisionBox(physicalWorld);
                collision.RectPosition = ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size.X > 0
                    ? ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Position : ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Vertexes[2];
                collision.RectSize = ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size.X > 0
                    ? ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size : -((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size;
                polygonObject.Dispose();
                AddObject(collision);
                UndoRedoManager.Enable = true;
                UndoRedoManager.ChangeObject2D(this, collision, true);
                CheckMapEventRect();
            }
        }

        void SetCollisionTriangle()
        {
            if (Mouse.LeftButton == asd.ButtonState.Push)
            {
                var temp = new asd.GeometryObject2D();
                temp.Shape = new asd.CircleShape()
                {
                    Position = GetMouseRelativePosition(),
                    OuterDiameter = 8,
                };
                temp.CameraGroup = 1;
                temp.Color = new asd.Color(255, 255, 255);
                dotObjects.Add(temp);
                AddObject(temp);

                if (dotObjects.Count == 3)
                {
                    UndoRedoManager.Enable = false;
                    CollisionTriangle collision = new CollisionTriangle(physicalWorld);
                    for (int i = 0; i < 3; i++)
                    {
                        collision.SetVertexesByIndex(((asd.CircleShape)dotObjects[i].Shape).Position, i);
                        dotObjects[i].Dispose();
                    }
                    dotObjects.Clear();
                    AddObject(collision);
                    UndoRedoManager.Enable = true;
                    UndoRedoManager.ChangeObject2D(this, collision, true);
                    CheckMapEventRect();
                }
            }
        }

        void SetDoor()
        {
            if (Mouse.LeftButton == asd.ButtonState.Push)
            {
                UndoRedoManager.Enable = false;
                Door door;
                door = new Door("Script/door_kasuga.csx");
                door.Position = GetMouseRelativePosition();
                door.ID = GetCanUseDoorID();
                door.MoveToPosition = new asd.Vector2DF();
                AddObject(door);
                UndoRedoManager.Enable = true;
                UndoRedoManager.ChangeObject2D(this, door, true);
                SelectedObject = door;
                OnCreateDoor();
            }
        }

        public int GetCanUseDoorID()
        {
            SortedList<int, bool> IsUseIDs = new SortedList<int, bool>();
            foreach (var item in Doors)
            {
                IsUseIDs.Add(item.ID, false);
            }
            int result = 0;
            while (IsUseIDs.ContainsKey(result))
            {
                result++;
            }
            return result;
        }

        void SetMapObject()
        {
            if (Mouse.LeftButton == asd.ButtonState.Push)
            {
                UndoRedoManager.Enable = false;
                MapObject mapObject;
                mapObject = new MapObject();
                mapObject.Position = GetMouseRelativePosition();
                if (Scene is MapEditor && ((MapEditor)Scene).SelectedTemplate as MapObjectTemplate != null)
                {
                    var template = ((MapEditor)Scene).SelectedTemplate as MapObjectTemplate;
                    mapObject.ScriptPath = template.ScriptPath;
                }
                AddObject(mapObject);
                UndoRedoManager.Enable = true;
                UndoRedoManager.ChangeObject2D(this, mapObject, true);
                SelectedObject = mapObject;
                OnCreateMapObject();
            }
        }

        void SetEventObject()
        {
            if (Mouse.LeftButton == asd.ButtonState.Push)
            {
                UndoRedoManager.Enable = false;
                EventObject mapObject;
                mapObject = new EventObject();
                mapObject.Position = GetMouseRelativePosition();
                if (Scene is MapEditor && ((MapEditor)Scene).SelectedTemplate as MapObjectTemplate != null)
                {
                    var template = ((MapEditor)Scene).SelectedTemplate as MapObjectTemplate;
                    mapObject.ScriptPath = template.ScriptPath;
                    mapObject.MotionPath = template.MotionPath;
                }
                mapObject.ID = GetCanUseEventObjectID();
                AddObject(mapObject);
                UndoRedoManager.Enable = true;
                UndoRedoManager.ChangeObject2D(this, mapObject, true);
                SelectedObject = mapObject;
                OnCreateMapObject();
            }
        }

        public int GetCanUseEventObjectID()
        {
            SortedList<int, bool> IsUseIDs = new SortedList<int, bool>();
            foreach (var item in EventObjects)
            {
                IsUseIDs.Add(item.ID, false);
            }
            int result = 0;
            while (IsUseIDs.ContainsKey(result))
            {
                result++;
            }
            return result;
        }

        void SetMapEvent()
        {
            if (!polygonObject.IsAlive && Mouse.LeftButton == asd.ButtonState.Push)
            {
                polygonObject = new asd.GeometryObject2D();
                polygonObject.Shape = new asd.RectangleShape()
                {
                    DrawingArea = new asd.RectF(GetMouseRelativePosition(), new asd.Vector2DF())
                };
                polygonObject.CameraGroup = 1;
                polygonObject.Color = new asd.Color(0, 0, 255, 100);
                AddObject(polygonObject);
            }
            else if (polygonObject.IsAlive && Mouse.LeftButton == asd.ButtonState.Hold)
            {
                ((asd.RectangleShape)polygonObject.Shape).DrawingArea
                    = new asd.RectF(((asd.RectangleShape)polygonObject.Shape).DrawingArea.Position, ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size + GetMouseMoveVector());
            }
            else if (polygonObject.IsAlive && Mouse.LeftButton == asd.ButtonState.Release)
            {
                UndoRedoManager.Enable = false;
                MapEvent.MapEvent mapEvent = new MapEvent.MapEvent(RequireOpenFileDialog, SearchCharacterDataPath, physicalWorld);
                mapEvent.ID = GetCanUseEventID();
                mapEvent.Position = ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size.X > 0
                    ? ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Position : ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Vertexes[2];
                mapEvent.Size = ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size.X > 0
                    ? ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size : -((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size;
                polygonObject.Dispose();
                mapEvent.SetInitCameraPosition();
                AddObject(mapEvent);
                UndoRedoManager.Enable = true;
                UndoRedoManager.ChangeObject2D(this, mapEvent, true);
                CheckMapEventRect();
            }
        }

        public int GetCanUseEventID()
        {
            SortedList<int, bool> IsUseIDs = new SortedList<int, bool>();
            foreach (var item in MapEvents)
            {
                IsUseIDs.Add(item.ID, false);
            }
            int result = 0;
            while (IsUseIDs.ContainsKey(result))
            {
                result++;
            }
            return result;
        }

        void SelectEventObject()
        {
            foreach (var item in CollisionBoxes)
            {
                item.IsDrawn = false;
            }
            foreach (var item in CollisionTriangles)
            {
                item.IsDrawn = false;
            }
            foreach (var item in MapObjects)
            {
                item.IsDrawn = false;
            }
            foreach (var item in Doors)
            {
                item.IsDrawn = false;
            }
            foreach (var item in SavePoints)
            {
                item.IsDrawn = false;
            }

            foreach (var eventObject in EventObjects)
            {
                if (eventObject.CollisionShape.GetIsCollidedWith(cursorShape) && Mouse.LeftButton == asd.ButtonState.Push)
                {
                    if (((MapEvent.MapEvent)SelectedObject).Actors.Any(obj => !obj.IsUseName && obj.ID == eventObject.ID)) return;
                    ((MapEvent.MapEvent)SelectedObject).AddEventObjectActor(eventObject.MotionPath, eventObject.ID, eventObject.Position);

                    CurrentToolType = ToolType.Select;
                }
            }
        }

        void SetCameraRestriction()
        {
            if (!polygonObject.IsAlive && Mouse.LeftButton == asd.ButtonState.Push)
            {
                polygonObject = new asd.GeometryObject2D();
                polygonObject.Shape = new asd.RectangleShape()
                {
                    DrawingArea = new asd.RectF(GetMouseRelativePosition(), new asd.Vector2DF())
                };
                polygonObject.CameraGroup = 1;
                polygonObject.Color = new asd.Color(0, 0, 255, 100);
                AddObject(polygonObject);
            }
            else if (polygonObject.IsAlive && Mouse.LeftButton == asd.ButtonState.Hold)
            {
                ((asd.RectangleShape)polygonObject.Shape).DrawingArea
                    = new asd.RectF(((asd.RectangleShape)polygonObject.Shape).DrawingArea.Position, ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size + GetMouseMoveVector());
            }
            else if (polygonObject.IsAlive && Mouse.LeftButton == asd.ButtonState.Release)
            {
                UndoRedoManager.Enable = false;
                CameraRestriction cameraRestriction = new CameraRestriction();
                cameraRestriction.RectPosition = ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size.X > 0
                    ? ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Position : ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Vertexes[2];
                cameraRestriction.RectSize = ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size.X > 0
                    ? ((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size : -((asd.RectangleShape)polygonObject.Shape).DrawingArea.Size;
                polygonObject.Dispose();
                AddObject(cameraRestriction);
                UndoRedoManager.Enable = true;
                UndoRedoManager.ChangeObject2D(this, cameraRestriction, true);
                CheckMapEventRect();
            }
        }

        void SetSavePoint()
        {
            if (Mouse.LeftButton == asd.ButtonState.Push)
            {
                UndoRedoManager.Enable = false;
                SavePoint savePoint;
                savePoint = new SavePoint();
                savePoint.Position = GetMouseRelativePosition();
                savePoint.ID = GetCanUseSavePointID();
                AddObject(savePoint);
                UndoRedoManager.Enable = true;
                UndoRedoManager.ChangeObject2D(this, savePoint, true);
                SelectedObject = savePoint;
                OnCreateMapObject();
            }
        }

        public int GetCanUseSavePointID()
        {
            SortedList<int, bool> IsUseIDs = new SortedList<int, bool>();
            foreach (var item in SavePoints)
            {
                IsUseIDs.Add(item.ID, false);
            }
            int result = 0;
            while (IsUseIDs.ContainsKey(result))
            {
                result++;
            }
            return result;
        }

        void SelectObject()
        {
            if (Mouse.LeftButton == asd.ButtonState.Push ||
                (SelectedObject != null && (!SelectedObject.IsAlive || SelectedObject.Layer == null)))
            {
                switch (GetSelectedObjectType())
                {
                    case SelectType.None:
                        break;
                    case SelectType.Box:
                        if (((CollisionBox)SelectedObject).Shape.GetIsCollidedWith(cursorShape)) return;
                        break;
                    case SelectType.Triangle:
                        if (((CollisionTriangle)SelectedObject).Shape.GetIsCollidedWith(cursorShape)) return;
                        break;
                    case SelectType.Door:
                        if (((Door)SelectedObject).CollisionShape.GetIsCollidedWith(cursorShape)) return;
                        break;
                    case SelectType.Object:
                        if (((MapObject)SelectedObject).CollisionShape.GetIsCollidedWith(cursorShape)) return;
                        break;
                    case SelectType.EventObject:
                        if (((EventObject)SelectedObject).CollisionShape.GetIsCollidedWith(cursorShape)) return;
                        break;
                    case SelectType.Event:
                        if (((MapEvent.MapEvent)SelectedObject).Shape.GetIsCollidedWith(cursorShape)) return;
                        ((MapEvent.MapEvent)SelectedObject).OnUnselected();
                        break;
                    case SelectType.CameraRestriction:
                        if (((CameraRestriction)SelectedObject).Shape.GetIsCollidedWith(cursorShape)) return;
                        break;
                    case SelectType.SavePoint:
                        if (((SavePoint)SelectedObject).Shape.GetIsCollidedWith(cursorShape)) return;
                        break;
                    default:
                        break;
                }
                SelectedObject = null;
            }
            else return;

            foreach (var item in EventObjects)
            {
                if (item.CollisionShape.GetIsCollidedWith(cursorShape)) SelectedObject = item;
            }
            foreach (var item in MapObjects)
            {
                if (item.CollisionShape.GetIsCollidedWith(cursorShape)) SelectedObject = item;
            }
            foreach (var item in Doors)
            {
                if (item.CollisionShape.GetIsCollidedWith(cursorShape)) SelectedObject = item;
            }
            foreach (var item in CollisionBoxes)
            {
                if (item.Shape.GetIsCollidedWith(cursorShape)) SelectedObject = item;
            }
            foreach (var item in CollisionTriangles)
            {
                if (item.Shape.GetIsCollidedWith(cursorShape)) SelectedObject = item;
            }
            foreach (var item in MapEvents)
            {
                if (item.Shape.GetIsCollidedWith(cursorShape)) SelectedObject = item;
            }
            foreach (var item in CameraRestrictions)
            {
                if (item.Shape.GetIsCollidedWith(cursorShape)) SelectedObject = item;
            }
            foreach (var item in SavePoints)
            {
                if (item.Shape.GetIsCollidedWith(cursorShape)) SelectedObject = item;
            }
            InitOperationTool();
            OnChangeSelectedObject();
        }

        void InitOperationTool()
        {
            foreach (var item in dotObjects)
            {
                item.Dispose();
            }
            dotObjects.Clear();
            polygonObject.Dispose();
            switch (GetSelectedObjectType())
            {
                case SelectType.None:
                    break;
                case SelectType.Box:
                    for (int i = 0; i < 4; i++)
                    {
                        var temp = new asd.GeometryObject2D();
                        temp.Shape = new asd.CircleShape()
                        {
                            Position = ((CollisionBox)SelectedObject).Shape.DrawingArea.Vertexes[i],
                            OuterDiameter = 8,
                        };
                        temp.CameraGroup = 1;
                        temp.Color = new asd.Color(255, 255, 255);
                        dotObjects.Add(temp);
                        AddObject(temp);
                    }
                    polygonObject = new asd.GeometryObject2D();
                    polygonObject.Shape = ((CollisionBox)SelectedObject).Shape;
                    polygonObject.CameraGroup = 1;
                    polygonObject.Color = new asd.Color(255, 0, 0, 50);
                    AddObject(polygonObject);
                    break;
                case SelectType.Triangle:
                    for (int i = 0; i < 3; i++)
                    {
                        var temp = new asd.GeometryObject2D();
                        temp.Shape = new asd.CircleShape()
                        {
                            Position = ((CollisionTriangle)SelectedObject).Shape.GetPointByIndex(i),
                            OuterDiameter = 8,
                        };
                        temp.CameraGroup = 1;
                        temp.Color = new asd.Color(255, 255, 255);
                        dotObjects.Add(temp);
                        AddObject(temp);
                    }
                    polygonObject = new asd.GeometryObject2D();
                    polygonObject.Shape = ((CollisionTriangle)SelectedObject).Shape;
                    polygonObject.CameraGroup = 1;
                    polygonObject.Color = new asd.Color(255, 0, 0, 50);
                    AddObject(polygonObject);
                    break;
                case SelectType.Door:
                    polygonObject = new asd.GeometryObject2D();
                    polygonObject.Shape = ((Door)SelectedObject).CollisionShape;
                    polygonObject.CameraGroup = 1;
                    polygonObject.Color = new asd.Color(255, 0, 0, 50);
                    AddObject(polygonObject);
                    break;
                case SelectType.Object:
                    polygonObject = new asd.GeometryObject2D();
                    polygonObject.Shape = ((MapObject)SelectedObject).CollisionShape;
                    polygonObject.CameraGroup = 1;
                    polygonObject.Color = new asd.Color(255, 0, 0, 50);
                    AddObject(polygonObject);
                    break;
                case SelectType.EventObject:
                    polygonObject = new asd.GeometryObject2D();
                    polygonObject.Shape = ((MapObject)SelectedObject).CollisionShape;
                    polygonObject.CameraGroup = 1;
                    polygonObject.Color = new asd.Color(255, 0, 0, 50);
                    AddObject(polygonObject);
                    break;
                case SelectType.Event:
                    for (int i = 0; i < 4; i++)
                    {
                        var temp = new asd.GeometryObject2D();
                        temp.Shape = new asd.CircleShape()
                        {
                            Position = ((MapEvent.MapEvent)SelectedObject).Shape.DrawingArea.Vertexes[i],
                            OuterDiameter = 8,
                        };
                        temp.CameraGroup = 1;
                        temp.Color = new asd.Color(255, 255, 255);
                        dotObjects.Add(temp);
                        AddObject(temp);
                    }
                    polygonObject = new asd.GeometryObject2D();
                    polygonObject.Shape = ((MapEvent.MapEvent)SelectedObject).Shape;
                    polygonObject.CameraGroup = 1;
                    polygonObject.Color = new asd.Color(255, 0, 0, 50);
                    AddObject(polygonObject);
                    ((MapEvent.MapEvent)SelectedObject).OnSelected();
                    break;
                case SelectType.CameraRestriction:
                    for (int i = 0; i < 4; i++)
                    {
                        var temp = new asd.GeometryObject2D();
                        temp.Shape = new asd.CircleShape()
                        {
                            Position = ((CameraRestriction)SelectedObject).Shape.DrawingArea.Vertexes[i],
                            OuterDiameter = 8,
                        };
                        temp.CameraGroup = 1;
                        temp.Color = new asd.Color(255, 255, 255);
                        dotObjects.Add(temp);
                        AddObject(temp);
                    }
                    polygonObject = new asd.GeometryObject2D();
                    polygonObject.Shape = ((CameraRestriction)SelectedObject).Shape;
                    polygonObject.CameraGroup = 1;
                    polygonObject.Color = new asd.Color(255, 0, 0, 50);
                    AddObject(polygonObject);
                    break;
                case SelectType.SavePoint:
                    polygonObject = new asd.GeometryObject2D();
                    polygonObject.Shape = ((SavePoint)SelectedObject).Shape;
                    polygonObject.CameraGroup = 1;
                    polygonObject.Color = new asd.Color(255, 0, 0, 50);
                    AddObject(polygonObject);
                    break;
                default:
                    break;
            }
        }

        bool OperationSelectedObject()
        {
            switch (GetSelectedObjectType())
            {
                case SelectType.None:
                    break;
                case SelectType.Box:
                    return OperationSelectedBox();
                case SelectType.Triangle:
                    return OperationSelectedTriangle();
                case SelectType.Door:
                    return OperationSelectedDoor();
                case SelectType.Object:
                    return OperationSelectedMapObject();
                case SelectType.EventObject:
                    return OperationSelectedMapObject();
                case SelectType.Event:
                    if (!((MapEvent.MapEvent)SelectedObject).GetIsActive()) return OperationSelectedMapEvent();
                    else
                    {
                        FocusToEditorPanel();
                        physicalWorld.Update();
                        OperationSelectedMapEvent();
                        return true;
                    }
                case SelectType.CameraRestriction:
                    return OperationCameraRestriction();
                case SelectType.SavePoint:
                    return OperationSelectedSavePoint();
                default:
                    break;
            }
            return false;
        }

        bool OperationSelectedBox()
        {
            for (int i = 0; i < 4; i++)
            {
                ((asd.CircleShape)dotObjects[i].Shape).Position = ((CollisionBox)SelectedObject).Shape.DrawingArea.Vertexes[i];
                dotObjects[i].Shape = dotObjects[i].Shape;
            }

            for (int i = 0; i < 4; i++)
            {
                var result = DragObject((CollisionBox)SelectedObject, dotObjects[i].Shape, () =>
                     {
                         ((asd.CircleShape)dotObjects[i].Shape).Position = GetMouseRelativePosition();
                         if (i % 2 != 0)
                         {
                             ((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position
                                 = new asd.Vector2DF(((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position.X, GetMouseRelativePosition().Y);
                             ((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position
                                 = new asd.Vector2DF(GetMouseRelativePosition().X, ((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position.Y);
                         }
                         else
                         {
                             ((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position
                                                         = new asd.Vector2DF(((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position.X, GetMouseRelativePosition().Y);
                             ((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position
                                 = new asd.Vector2DF(GetMouseRelativePosition().X, ((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position.Y);
                         }

                         ((CollisionBox)SelectedObject).RectPosition = new asd.Vector2DF(
                             Math.Min(((asd.CircleShape)dotObjects[0].Shape).Position.X, ((asd.CircleShape)dotObjects[1].Shape).Position.X),
                             Math.Min(((asd.CircleShape)dotObjects[1].Shape).Position.Y, ((asd.CircleShape)dotObjects[2].Shape).Position.Y));
                         ((CollisionBox)SelectedObject).RectSize = new asd.Vector2DF(
                             Math.Abs(((asd.CircleShape)dotObjects[0].Shape).Position.X - ((asd.CircleShape)dotObjects[1].Shape).Position.X),
                             Math.Abs(((asd.CircleShape)dotObjects[1].Shape).Position.Y - ((asd.CircleShape)dotObjects[2].Shape).Position.Y));
                         polygonObject.Shape = polygonObject.Shape;
                     });
                if (result) return true;
            }

            return DragObject((CollisionBox)SelectedObject, polygonObject.Shape, () =>
                  {
                      ((CollisionBox)SelectedObject).RectPosition += GetMouseMoveVector();
                      for (int i = 0; i < 4; i++)
                      {
                          ((asd.CircleShape)dotObjects[i].Shape).Position = ((CollisionBox)SelectedObject).Shape.DrawingArea.Vertexes[i];
                          dotObjects[i].Shape = dotObjects[i].Shape;
                      }
                      polygonObject.Shape = polygonObject.Shape;
                  });
        }

        bool OperationSelectedTriangle()
        {
            for (int i = 0; i < 3; i++)
            {
                ((asd.CircleShape)dotObjects[i].Shape).Position = ((CollisionTriangle)SelectedObject).Shape.GetPointByIndex(i);
                dotObjects[i].Shape = dotObjects[i].Shape;
            }

            for (int i = 0; i < 3; i++)
            {
                var result = DragObject((CollisionTriangle)SelectedObject, dotObjects[i].Shape, () =>
                     {
                         ((asd.CircleShape)dotObjects[i].Shape).Position = GetMouseRelativePosition();
                         ((CollisionTriangle)SelectedObject).SetVertexesByIndex(GetMouseRelativePosition(), i);
                         polygonObject.Shape = polygonObject.Shape;
                     });
                if (result) return true;
            }

            return DragObject((CollisionTriangle)SelectedObject, polygonObject.Shape, () =>
                 {
                     for (int i = 0; i < 3; i++)
                     {
                         ((CollisionTriangle)SelectedObject).SetVertexesByIndex(((CollisionTriangle)SelectedObject).Shape.GetPointByIndex(i) + GetMouseMoveVector(), i);
                         ((asd.CircleShape)dotObjects[i].Shape).Position += GetMouseMoveVector();
                         dotObjects[i].Shape = dotObjects[i].Shape;
                     }
                     polygonObject.Shape = polygonObject.Shape;
                 });
        }

        bool OperationSelectedDoor()
        {
            return DragObject((Door)SelectedObject, ((Door)SelectedObject).CollisionShape, () =>
                 {
                     ((Door)SelectedObject).Position += GetMouseMoveVector();
                 });
        }

        bool OperationSelectedMapObject()
        {
            return DragObject((MapObject)SelectedObject, ((MapObject)SelectedObject).CollisionShape, () =>
                 {
                     ((MapObject)SelectedObject).Position += GetMouseMoveVector();
                 });
        }

        private bool OperationSelectedMapEvent()
        {
            for (int i = 0; i < 4; i++)
            {
                ((asd.CircleShape)dotObjects[i].Shape).Position = ((MapEvent.MapEvent)SelectedObject).Shape.DrawingArea.Vertexes[i];
                dotObjects[i].Shape = dotObjects[i].Shape;
            }

            for (int i = 0; i < 4; i++)
            {
                var result = DragObject((MapEvent.MapEvent)SelectedObject, dotObjects[i].Shape, () =>
                {
                    ((asd.CircleShape)dotObjects[i].Shape).Position = GetMouseRelativePosition();
                    if (i % 2 != 0)
                    {
                        ((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position
                            = new asd.Vector2DF(((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position.X, GetMouseRelativePosition().Y);
                        ((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position
                            = new asd.Vector2DF(GetMouseRelativePosition().X, ((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position.Y);
                    }
                    else
                    {
                        ((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position
                                                    = new asd.Vector2DF(((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position.X, GetMouseRelativePosition().Y);
                        ((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position
                            = new asd.Vector2DF(GetMouseRelativePosition().X, ((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position.Y);
                    }

                        ((MapEvent.MapEvent)SelectedObject).Position = new asd.Vector2DF(
                            Math.Min(((asd.CircleShape)dotObjects[0].Shape).Position.X, ((asd.CircleShape)dotObjects[1].Shape).Position.X),
                            Math.Min(((asd.CircleShape)dotObjects[1].Shape).Position.Y, ((asd.CircleShape)dotObjects[2].Shape).Position.Y));
                    ((MapEvent.MapEvent)SelectedObject).Size = new asd.Vector2DF(
                        Math.Abs(((asd.CircleShape)dotObjects[0].Shape).Position.X - ((asd.CircleShape)dotObjects[1].Shape).Position.X),
                        Math.Abs(((asd.CircleShape)dotObjects[1].Shape).Position.Y - ((asd.CircleShape)dotObjects[2].Shape).Position.Y));
                    polygonObject.Shape = polygonObject.Shape;
                });
                if (result) return true;
            }

            return DragObject((MapEvent.MapEvent)SelectedObject, polygonObject.Shape, () =>
            {
                ((MapEvent.MapEvent)SelectedObject).Position += GetMouseMoveVector();
                for (int i = 0; i < 4; i++)
                {
                    ((asd.CircleShape)dotObjects[i].Shape).Position = ((MapEvent.MapEvent)SelectedObject).Shape.DrawingArea.Vertexes[i];
                    dotObjects[i].Shape = dotObjects[i].Shape;
                }
                polygonObject.Shape = polygonObject.Shape;
            });
        }

        bool OperationCameraRestriction()
        {
            for (int i = 0; i < 4; i++)
            {
                ((asd.CircleShape)dotObjects[i].Shape).Position = ((CameraRestriction)SelectedObject).Shape.DrawingArea.Vertexes[i];
                dotObjects[i].Shape = dotObjects[i].Shape;
            }

            for (int i = 0; i < 4; i++)
            {
                var result = DragObject((CameraRestriction)SelectedObject, dotObjects[i].Shape, () =>
                {
                    ((asd.CircleShape)dotObjects[i].Shape).Position = GetMouseRelativePosition();
                    if (i % 2 != 0)
                    {
                        ((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position
                            = new asd.Vector2DF(((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position.X, GetMouseRelativePosition().Y);
                        ((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position
                            = new asd.Vector2DF(GetMouseRelativePosition().X, ((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position.Y);
                    }
                    else
                    {
                        ((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position
                                                    = new asd.Vector2DF(((asd.CircleShape)dotObjects[(i + 1) % 4].Shape).Position.X, GetMouseRelativePosition().Y);
                        ((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position
                            = new asd.Vector2DF(GetMouseRelativePosition().X, ((asd.CircleShape)dotObjects[(i + 3) % 4].Shape).Position.Y);
                    }

                         ((CameraRestriction)SelectedObject).RectPosition = new asd.Vector2DF(
                             Math.Min(((asd.CircleShape)dotObjects[0].Shape).Position.X, ((asd.CircleShape)dotObjects[1].Shape).Position.X),
                             Math.Min(((asd.CircleShape)dotObjects[1].Shape).Position.Y, ((asd.CircleShape)dotObjects[2].Shape).Position.Y));
                    ((CameraRestriction)SelectedObject).RectSize = new asd.Vector2DF(
                        Math.Abs(((asd.CircleShape)dotObjects[0].Shape).Position.X - ((asd.CircleShape)dotObjects[1].Shape).Position.X),
                        Math.Abs(((asd.CircleShape)dotObjects[1].Shape).Position.Y - ((asd.CircleShape)dotObjects[2].Shape).Position.Y));
                    polygonObject.Shape = polygonObject.Shape;
                });
                if (result) return true;
            }

            return DragObject((CameraRestriction)SelectedObject, polygonObject.Shape, () =>
            {
                ((CameraRestriction)SelectedObject).RectPosition += GetMouseMoveVector();
                for (int i = 0; i < 4; i++)
                {
                    ((asd.CircleShape)dotObjects[i].Shape).Position = ((CameraRestriction)SelectedObject).Shape.DrawingArea.Vertexes[i];
                    dotObjects[i].Shape = dotObjects[i].Shape;
                }
                polygonObject.Shape = polygonObject.Shape;
            });
        }

        bool OperationSelectedSavePoint()
        {
            return DragObject((SavePoint)SelectedObject, ((SavePoint)SelectedObject).Shape, () =>
            {
                ((SavePoint)SelectedObject).Position += GetMouseMoveVector();
            });
        }

        bool DragObject(IMovable movable, asd.Shape shape, Action func)
        {
            if (shape.GetIsCollidedWith(cursorShape)
                    && Mouse.LeftButton == asd.ButtonState.Push)
            {
                movable.StartMove();
                dragObject = shape;
                return true;
            }
            if (Mouse.LeftButton == asd.ButtonState.Hold && dragObject == shape)
            {
                func();
                return true;
            }
            if (Mouse.LeftButton == asd.ButtonState.Release && dragObject == shape)
            {
                dragObject = null;
                func();
                movable.EndMove();
                CheckMapEventRect();
                return true;
            }
            return false;
        }

        void CheckMapEventRect()
        {
            foreach (var item in MapEvents)
            {
                bool isEnd = false;
                foreach (var item2 in CollisionTriangles)
                {
                    if (item2.Shape.GetIsCollidedWith(item.Shape))
                    {
                        item.Color = new asd.Color(255, 0, 0, 100);
                        isEnd = true;
                        break;
                    }
                }
                if (isEnd) continue;
                if (CollisionBoxes.Count(obj => obj.Shape.GetIsCollidedWith(item.Shape)) != 1)
                {
                    item.Color = new asd.Color(255, 0, 0, 100);
                    continue;
                }
                if (CollisionBoxes.Count(obj =>
                    obj.Shape.GetIsCollidedWith(new asd.CircleShape()
                    {
                        Position = item.Shape.DrawingArea.Vertexes[2],
                        OuterDiameter = 3,
                    })) != 1)
                {
                    item.Color = new asd.Color(255, 0, 0, 100);
                    continue;
                }
                if (CollisionBoxes.Count(obj =>
                        obj.Shape.GetIsCollidedWith(new asd.CircleShape()
                        {
                            Position = item.Shape.DrawingArea.Vertexes[3],
                            OuterDiameter = 3,
                        })) != 1)
                {
                    item.Color = new asd.Color(255, 0, 0, 100);
                    continue;
                }
                item.Color = new asd.Color(0, 255, 0, 100);
            }
        }

        asd.Vector2DF GetMouseRelativePosition()
        {
            return ScrollCamera.Src.Position.To2DF() + Mouse.Position * ScrollCamera.Src.Width / ScrollCamera.Dst.Width;
        }

        asd.Vector2DF GetMouseMoveVector()
        {
            return (Mouse.Position - preMousePosition) * ScrollCamera.Src.Width / ScrollCamera.Dst.Width;
        }
    }

    public enum ToolType
    {
        Select,
        Box,
        Triangle,
        Door,
        Object,
        EventObject,
        Event,
        SelectEventObject,
        CameraRestriction,
        SavePoint,
    }

    public enum SelectType
    {
        None,
        Box,
        Triangle,
        Door,
        Object,
        EventObject,
        Event,
        CameraRestriction,
        SavePoint,
    }
}
