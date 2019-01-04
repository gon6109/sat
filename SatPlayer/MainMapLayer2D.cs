using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using PhysicAltseed;
using BaseComponent;
using System.Collections.Concurrent;

namespace SatPlayer
{
    public class MainMapLayer2D : asd.Layer2D, IDamageManeger
    {
        public ScrollCamera PlayerCamera { get; private set; }

        public PhysicalWorld PhysicalWorld { get; protected set; }

        public Player Player { get; protected set; }

        public List<PhysicalShape> CollisionShapes { get; private set; }

        public List<NPCMapObject> NpcMapObjects => Objects.Where(obj => obj is NPCMapObject).Cast<NPCMapObject>().ToList();

        public List<MapObject> MapObjects => Objects.Where(obj => obj is MapObject && !(obj is NPCMapObject)).Cast<MapObject>().ToList();

        public List<Door> Doors => Objects.Where(obj => obj is Door).Cast<Door>().ToList();

        public List<SavePoint> SavePoints => Objects.Where(obj => obj is SavePoint).Cast<SavePoint>().ToList();

        public List<MapEvent.MapEvent> MapEvents => Objects.Where(obj => obj is MapEvent.MapEvent).Cast<MapEvent.MapEvent>().ToList();

        public List<DamageRect> Damages { get; private set; }

        public MainMapLayer2D(Player refPlayer)
        {
            Player = refPlayer;
            CollisionShapes = new List<PhysicalShape>();
            Damages = new List<DamageRect>();
        }

        public MainMapLayer2D()
        {
            CollisionShapes = new List<PhysicalShape>();
            Damages = new List<DamageRect>();
        }

        public int ElementCount { get; set; }
        public int LoadingElementCount { get; set; }

        public IEnumerator<int> LoadMapData(BlockingCollection<Action> subThreadQueue, BlockingCollection<Action> mainThreadQueue, SatIO.MapIO mapIO, int initDoorID, int initSavePointID)
        {
            foreach (var item in mapIO.BackGrounds)
            {
                AddObject(BackGround.LoadBackGroud(item, this));
                LoadingElementCount++;
                yield return 0;
            }

            PhysicalWorld = new PhysicalWorld(new asd.RectF(new asd.Vector2DF(-200, -200), mapIO.Size + new asd.Vector2DF(200, 200) * 2), new asd.Vector2DF(0, 2000));

            {
                PlayerCamera = new ScrollCamera(mapIO.CameraRestrictions);
                PlayerCamera.HomingObject = Player;
                PlayerCamera.Src = new asd.RectI(0, 0, Base.ScreenSize.X, Base.ScreenSize.Y);
                PlayerCamera.MapSize = mapIO.Size;
                AddObject(PlayerCamera);
            }

            foreach (var item in mapIO.CollisionBoxes)
            {
                PhysicalRectangleShape temp = new PhysicalRectangleShape(PhysicalShapeType.Static, PhysicalWorld);
                temp.Density = 1;
                temp.Restitution = 0;
                temp.Friction = 0;
                temp.DrawingArea = new asd.RectF(item.Position, item.Size);
                CollisionShapes.Add(temp);
#if DEBUG
                asd.GeometryObject2D geometryObject = new asd.GeometryObject2D();
                geometryObject.CameraGroup = 1;
                geometryObject.Shape = temp;
                geometryObject.Color = new asd.Color(0, 0, 255, 100);
                geometryObject.DrawingPriority = 2;
                AddObject(geometryObject);
#endif
            }

            foreach (var item in mapIO.CollisionTriangles)
            {
                PhysicalTriangleShape temp = new PhysicalTriangleShape(PhysicalShapeType.Static, PhysicalWorld);
                temp.Density = 1;
                temp.Restitution = 0;
                temp.Friction = 0;
                var i = 0;
                foreach (var vertex in item.vertexes)
                {
                    temp.SetPointByIndex(vertex, i);
                    i++;
                }
                CollisionShapes.Add(temp);

#if DEBUG
                asd.GeometryObject2D geometryObject = new asd.GeometryObject2D();
                geometryObject.CameraGroup = 1;
                geometryObject.Shape = temp;
                geometryObject.Color = new asd.Color(0, 0, 255, 100);
                geometryObject.DrawingPriority = 2;
                AddObject(geometryObject);
#endif
            }

            var tempDoors = new List<Door>();
            foreach (var item in mapIO.Doors)
            {
                Door temp = new Door(subThreadQueue, item.TexturePath, item.KeyScriptPath, Player);
                temp.Position = item.Position;
                temp.ID = item.ID;
                temp.IsUseMoveToID = item.IsUseMoveToID;
                temp.MoveToMap = item.MoveToMap;
                temp.MoveToID = item.MoveToID;
                AddObject(temp);
                tempDoors.Add(temp);
                yield return 0;
                LoadingElementCount++;
            }

            foreach (var item in mapIO.MapObjects)
            {
                try
                {
                    MapObject temp = new MapObject(subThreadQueue, mainThreadQueue, item.ScriptPath, PhysicalWorld);
                    temp.Position = item.Position;
                    AddObject(temp);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
                yield return 0;
                LoadingElementCount++;
            }

            List<IMotion> actors = new List<IMotion>(Game.Players);
            foreach (var item in mapIO.NPCMapObjects)
            {
                try
                {
                    NPCMapObject temp = new NPCMapObject(subThreadQueue, mainThreadQueue, item.ScriptPath, PhysicalWorld, item.MotionPath);
                    temp.Position = item.Position;
                    AddObject(temp);
                    actors.Add(temp);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
                yield return 0;
                LoadingElementCount++;
            }

            foreach (var item in mapIO.MapEvents)
            {
                if (Game.EndEvents.Any(obj => obj.Key == ((Game)Scene).MapPath && obj.Value == item.ID)) continue;
                try
                {
                    bool isSkip = false;
                    foreach (var item2 in item.Actors.Where(obj => obj.IsUseName))
                    {
                        if (!((Game)Scene).CanUsePlayers.Any(obj => obj.Name == item2.Name))
                        {
                            isSkip = true;
                            break;
                        }
                    }
                    if (isSkip) continue;
                    MapEvent.MapEvent temp = new MapEvent.MapEvent(item, actors, PlayerCamera);
                    AddObject(temp);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
                yield return 0;
                LoadingElementCount++;
            }

            List<SavePoint> tempSavePoints = new List<SavePoint>();
            foreach (var item in mapIO.SavePoints)
            {
                SavePoint savePoint = new SavePoint(item);
                AddObject(savePoint);
                tempSavePoints.Add(savePoint);
            }

            if (initSavePointID != -1 && tempSavePoints.Find(savePoint => savePoint.ID == initSavePointID) != null)
                Player.Position = tempSavePoints.Find(savePoint => savePoint.ID == initSavePointID).Position;
            else if (initDoorID != -1 && tempDoors.Find(door => door.ID == initDoorID) != null)
                Player.Position = tempDoors.Find(door => door.ID == initDoorID).Position;
        }

        protected override void OnAdded()
        {
            int count = 15;
            foreach (var item in ((Game)Scene).CanUsePlayers)
            {
                if (item == Player) continue;
                for (int i = 0; i < count; i++)
                {
                    item.MoveCommands.Enqueue(new Dictionary<BaseComponent.Inputs, bool>());
                }
                item.Color = new asd.Color(100, 100, 100);
            }
            base.OnAdded();
        }

        protected override void OnUpdating()
        {
            base.OnUpdating();

            Dictionary<Inputs, bool> key = new Dictionary<Inputs, bool>();
            foreach (Inputs item in Enum.GetValues(typeof(Inputs)))
            {
                key[item] = Input.GetInputState(item) > 0;
            }
            foreach (var item in ((Game)Scene).CanUsePlayers)
            {
                if (item == Player) continue;
                item.MoveCommands.Enqueue(key);
            }

            if (!SavePoints.Any(obj => obj.IsActive)) PhysicalWorld?.Update();
        }

        protected override void OnUpdated()
        {
            if (!MapEvents.Any(obj => obj.IsUpdated == true))
            {
                foreach (var item in MapEvents)
                {
                    if (Player.CollisionShape.GetIsCollidedWith(item.Shape)) item.IsUpdated = true;
                }
            }

            foreach (var item in Doors)
            {
                if (item.AcceptLeave())
                {
                    if (item.MoveToMap != null && asd.Engine.File.Exists(item.MoveToMap))
                    {
                        var scene = asd.Engine.CurrentScene as Game;
                        scene?.OnChangeMapEvent
                            (item.MoveToMap, 
                            scene.CanUsePlayers, 
                            item.IsUseMoveToID ? new asd.Vector2DF() : item.MoveToPosition,
                            item.IsUseMoveToID ? item.MoveToID : -1);
                        IsUpdated = false;
                    }
                    else
                    {
                        var door = Doors.Find((Door obj) => obj.ID == item.MoveToID);
                        if (door != null)
                        {
                            door.AcceptCome();
                            Player.Position = item.IsUseMoveToID ? door.Position : item.MoveToPosition;
                        }
                    }
                }
            }

            UpdateDamage();

            base.OnUpdated();
        }

        void UpdateDamage()
        {
            foreach (IDamageControler item in Objects.Where(obj => obj is IDamageControler))
            {
                while (item.DamageRequests.Count > 0)
                {
                    Damages.Add(item.DamageRequests.Dequeue());
                }

                while (item.DirectDamageRequests.Count > 0)
                {
                    var damage = item.DirectDamageRequests.Dequeue();
                    damage.RecieveTo.HP -= damage.Damage;
                }
            }

            List<DamageRect> removeRect = new List<DamageRect>();
            foreach (var item in Damages.Where(obj => obj.Owner == DamageRect.OwnerType.Enemy))
            {
                if (Player.CollisionShape.GetIsCollidedWith(item))
                {
                    Player.HP -= item.Damage;
                    if (!item.Sastainable) removeRect.Add(item);
                }
            }

            foreach (var item in Damages.Where(obj => obj.Owner == DamageRect.OwnerType.Player))
            {
                foreach (IDamageControler item2 in Objects.Where(obj =>
                    obj is IDamageControler &&
                    ((IDamageControler)obj).IsReceiveDamage &&
                    ((IDamageControler)obj).OwnerType == DamageRect.OwnerType.Enemy &&
                    ((IDamageControler)obj).CollisionShape.GetIsCollidedWith(item)))
                {
                    item2.HP -= item.Damage;
                    if (!item.Sastainable) removeRect.Add(item);
                }
            }

            foreach (var item in Damages)
            {
                item.Frame--;
                if (item.Frame < 0) removeRect.Add(item);
            }

            foreach (var item in removeRect)
            {
                Damages.Remove(item);
            }
        }
    }
}
