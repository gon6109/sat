using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using PhysicAltseed;
using BaseComponent;
using System.Collections.Concurrent;
using SatPlayer.Game.Object;
using System.Threading.Tasks;

namespace SatPlayer.Game
{
    /// <summary>
    /// メインレイヤー
    /// </summary>
    public class MapLayer : ScalingLayer2D, IDamageManeger
    {
        /// <summary>
        /// プレイヤー用カメラ
        /// </summary>
        public ScrollCamera PlayerCamera { get; private set; }

        /// <summary>
        /// 物理エンジン用ワールド
        /// </summary>
        public PhysicalWorld PhysicalWorld { get; protected set; }

        /// <summary>
        /// 操作中のプレイヤー
        /// </summary>
        public Player Player { get; protected set; }

        /// <summary>
        /// 障害物
        /// </summary>
        public List<PhysicalShape> Obstacles { get; private set; }

        /// <summary>
        /// イベント用オブジェクト
        /// </summary>
        public IEnumerable<EventObject> EventObjects => Objects.OfType<EventObject>();

        /// <summary>
        /// マップオブジェクト(EventObject以外)
        /// </summary>
        public IEnumerable<MapObject> MapObjects => Objects.Where(obj => obj is MapObject && !(obj is EventObject)).OfType<MapObject>();

        /// <summary>
        /// ドア
        /// </summary>
        public IEnumerable<Door> Doors => Objects.OfType<Door>();

        /// <summary>
        /// セーブポイント
        /// </summary>
        public IEnumerable<SavePoint> SavePoints => Objects.OfType<SavePoint>();

        /// <summary>
        /// 強制イベント
        /// </summary>
        public IEnumerable<Object.MapEvent.MapEvent> MapEvents => Objects.OfType<Object.MapEvent.MapEvent>();

        /// <summary>
        /// ダメージ発生領域
        /// </summary>
        public List<DamageRect> Damages { get; private set; }

        public MapLayer(Player refPlayer)
        {
            Player = refPlayer;
            Obstacles = new List<PhysicalShape>();
            Damages = new List<DamageRect>();
        }

        public MapLayer()
        {
            Obstacles = new List<PhysicalShape>();
            Damages = new List<DamageRect>();
        }

        /// <summary>
        /// マップのロード
        /// </summary>
        /// <param name="mapIO">マップデータ</param>
        /// <param name="initDoorID">初期ドアID</param>
        /// <param name="initSavePointID">初期セーブポイント</param>
        /// <param name="info">ロード情報</param>
        /// <returns></returns>
        public async Task LoadMapData(SatIO.MapIO mapIO, int initDoorID, int initSavePointID, (int taskCount, int progress) info)
        {
            foreach (var item in mapIO.BackGrounds)
            {
                var backGround = await BackGround.CreateBackGroudAsync(item);
                AddObject(backGround);
                info.taskCount++;
            }

            PhysicalWorld = new PhysicalWorld(new asd.RectF(new asd.Vector2DF(-200, -200), mapIO.Size + new asd.Vector2DF(200, 200) * 2), new asd.Vector2DF(0, 2000));

            {
                PlayerCamera = new ScrollCamera(mapIO.CameraRestrictions);
                PlayerCamera.HomingObject = Player;
                PlayerCamera.Src = new asd.RectI(0, 0, (int)OriginDisplaySize.X, (int)OriginDisplaySize.Y);
                PlayerCamera.MapSize = mapIO.Size;
                AddObject(PlayerCamera);

                Camera.IsDrawn = false;
                Camera.IsUpdated = false;
            }

            foreach (var item in mapIO.CollisionBoxes)
            {
                PhysicalRectangleShape temp = new PhysicalRectangleShape(PhysicalShapeType.Static, PhysicalWorld);
                temp.Density = 1;
                temp.Restitution = 0;
                temp.Friction = 0;
                temp.DrawingArea = new asd.RectF(item.Position, item.Size);
                Obstacles.Add(temp);
                info.taskCount++;
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
                Obstacles.Add(temp);
                info.taskCount++;

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
                var door = await Door.CreateDoorAsync(item);
                AddObject(door);
                tempDoors.Add(door);
                info.taskCount++;
            }

            foreach (var item in mapIO.MapObjects)
            {
                try
                {
                    var mapObject = await MapObject.CreateMapObjectAsync(item);
                    AddObject(mapObject);
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
                info.taskCount++;
            }

            List<IActor> actors = new List<IActor>(GameScene.Players);
            foreach (var item in mapIO.EventObjects)
            {
                try
                {
                    var eventObject = await EventObject.CreateEventObjectAsync(item);
                    AddObject(eventObject);
                    actors.Add(eventObject);
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
                info.taskCount++;
            }

            if (Scene is GameScene gameScene)
            {
                foreach (var item in mapIO.MapEvents)
                {
                    if (GameScene.EndEvents.Any(obj => obj.Key == gameScene.MapPath && obj.Value == item.ID)) continue;
                    try
                    {
                        bool isSkip = false;
                        foreach (var item2 in item.Actors.Where(obj => obj.IsUseName))
                        {
                            if (!gameScene.CanUsePlayers.Any(obj => obj.Name == item2.Name))
                            {
                                isSkip = true;
                                break;
                            }
                        }
                        if (isSkip) continue;
                        Object.MapEvent.MapEvent temp = await Object.MapEvent.MapEvent.CreateMapEventAsync(item, actors, PlayerCamera);
                        AddObject(temp);
                    }
                    catch (Exception e)
                    {
                        ErrorIO.AddError(e);
                    }
                    info.taskCount++;
                }
            }

            List<SavePoint> tempSavePoints = new List<SavePoint>();
            foreach (var item in mapIO.SavePoints)
            {
                SavePoint savePoint = new SavePoint(item);
                AddObject(savePoint);
                tempSavePoints.Add(savePoint);
                info.taskCount++;
            }

            if (initSavePointID != -1 && tempSavePoints.Find(savePoint => savePoint.ID == initSavePointID) != null)
                Player.Position = tempSavePoints.Find(savePoint => savePoint.ID == initSavePointID).Position;
            else if (initDoorID != -1 && tempDoors.Find(door => door.ID == initDoorID) != null)
                Player.Position = tempDoors.Find(door => door.ID == initDoorID).Position;

            return;
        }

        protected override void OnAdded()
        {
            int count = 15;
            if (Scene is GameScene gameScene)
            {
                foreach (var item in gameScene.CanUsePlayers)
                {
                    if (item == Player) continue;
                    for (int i = 0; i < count; i++)
                    {
                        item.MoveCommands.Enqueue(new Dictionary<Inputs, bool>());
                    }
                    item.Color = new asd.Color(100, 100, 100);
                }
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

            if (Scene is GameScene gameScene)
            {
                foreach (var item in gameScene.CanUsePlayers)
                {
                    if (item == Player) continue;
                    item.MoveCommands.Enqueue(key);
                }
            }
            if (!SavePoints.Any(obj => obj.IsActive)) PhysicalWorld?.Update();
            UpdateCollision();
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
                        var scene = asd.Engine.CurrentScene as GameScene;
                        scene?.OnChangeMapEvent
                            (item.MoveToMap,
                            scene.CanUsePlayers,
                            item.IsUseMoveToID ? new asd.Vector2DF() : item.MoveToPosition,
                            item.IsUseMoveToID ? item.MoveToID : -1);
                        IsUpdated = false;
                    }
                    else
                    {
                        var door = Doors.FirstOrDefault((obj) => obj.ID == item.MoveToID);
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

        protected void UpdateCollision()
        {
            //初期化
            if (Player != null) Player.Collision = new Collision();
            foreach (var item in Objects.OfType<MapObject>())
            {
                item.Collision = new Collision();
                foreach (var item2 in item.Sensors)
                {
                    if (item2.Value is MapObject.Sensor sensor) sensor.Collision = new Collision();
                }
            }

            //Player<=>MapObject,EventObject
            if (Player?.Collision is Collision playerCollision) playerCollision.ColligingMapObjectTags = Objects.OfType<MapObject>().Where(obj =>
            {
                bool result = Player.CollisionShape.GetIsCollidedWith(obj.GetCoreShape());
                if (obj.Collision is Collision collision) collision.IsCollidedWithPlayer = result;
                return result;
            }).Select(obj => obj.Tag).Distinct().ToList();

            //Player=>Obstacle
            if (Player?.Collision is Collision playerCollision2) playerCollision2.IsCollidedWithObstacle = Obstacles.Any(obj => Player.CollisionShape.GetIsCollidedWith(obj));

            //MapObject,EventObject=>Obstacle
            foreach (var item in Objects.OfType<MapObject>())
            {
                if (item.Collision is Collision mapObjectCollision) mapObjectCollision.IsCollidedWithObstacle = Obstacles.Any(obj => obj.GetIsCollidedWith(item.GetCoreShape()));
            }

            //Sensor=>All
            foreach (var item in Objects.OfType<MapObject>().SelectMany(obj => obj.Sensors).OfType<MapObject.Sensor>())
            {
                if (item.Collision is Collision collision)
                {
                    collision.IsCollidedWithObstacle = Obstacles.Any(obj => item.GetIsCollidedWith(obj));
                    if (Player != null) collision.IsCollidedWithPlayer = item.GetIsCollidedWith((PhysicalShape)Player.CollisionShape);
                    collision.ColligingMapObjectTags = Objects.OfType<MapObject>().Where(obj =>
                        item.GetIsCollidedWith(obj.GetCoreShape())).Select(obj => obj.Tag).Distinct().ToList();
                }
            }

            //MapObject=>MapObject
            foreach (var item in Objects.OfType<MapObject>())
            {
                if (item.Collision is Collision collision)
                {
                    collision.ColligingMapObjectTags = Objects.OfType<MapObject>().Where(obj =>
                        obj != item && item.GetCoreShape().GetIsCollidedWith(obj.GetCoreShape())).Select(obj => obj.Tag).Distinct().ToList();
                }
            }
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
