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

        public MapLayer(Player player)
        {
            Player = player;
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
        /// <param name="loader">ロードするオブジェクト</param>
        /// <returns></returns>
        public async Task LoadMapData(SatIO.MapIO mapIO, int initDoorID, int initSavePointID, ILoader loader)
        {
            //背景
            foreach (var item in mapIO.BackGrounds)
            {
                var backGround = await BackGround.CreateBackGroudAsync(item);
                AddObject(backGround);
                loader.ProgressInfo = (loader.ProgressInfo.taskCount, loader.ProgressInfo.progress + 1);
            }

            //物理世界構築
            PhysicalWorld = new PhysicalWorld(new asd.RectF(new asd.Vector2DF(-200, -200), mapIO.Size + new asd.Vector2DF(200, 200) * 2), new asd.Vector2DF(0, 8000));

            //カメラ設定
            {
                PlayerCamera = new ScrollCamera(mapIO.CameraRestrictions);
                PlayerCamera.HomingObject = Player;
                PlayerCamera.Src = new asd.RectI(0, 0, (int)OriginDisplaySize.X, (int)OriginDisplaySize.Y);
                PlayerCamera.MapSize = mapIO.Size;
                AddObject(PlayerCamera);

                Camera.IsDrawn = false;
                Camera.IsUpdated = false;
            }

            //障害物
            foreach (var item in mapIO.CollisionBoxes)
            {
                PhysicalRectangleShape temp = new PhysicalRectangleShape(PhysicalShapeType.Static, PhysicalWorld);
                temp.Density = 1;
                temp.Restitution = 0;
                temp.Friction = 0;
                temp.DrawingArea = new asd.RectF(item.Position, item.Size);
                Obstacles.Add(temp);
                loader.ProgressInfo = (loader.ProgressInfo.taskCount, loader.ProgressInfo.progress + 1);
#if DEBUG
                asd.GeometryObject2D geometryObject = new asd.GeometryObject2D();
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
                loader.ProgressInfo = (loader.ProgressInfo.taskCount, loader.ProgressInfo.progress + 1);
#if DEBUG
                asd.GeometryObject2D geometryObject = new asd.GeometryObject2D();
                geometryObject.Shape = temp;
                geometryObject.Color = new asd.Color(0, 0, 255, 100);
                geometryObject.DrawingPriority = 2;
                AddObject(geometryObject);
#endif
            }

            //ドア
            var tempDoors = new List<Door>();
            foreach (var item in mapIO.Doors)
            {
                var door = await Door.CreateDoorAsync(item);
                door.OnLeave += OnLeave;
                AddObject(door);
                tempDoors.Add(door);
                loader.ProgressInfo = (loader.ProgressInfo.taskCount, loader.ProgressInfo.progress + 1);
            }

            //マップオブジェクト
            foreach (var item in mapIO.MapObjects)
            {
                try
                {
                    var mapObject = await MapObject.CreateMapObjectAsync(item);
                    AddObject(mapObject);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                loader.ProgressInfo = (loader.ProgressInfo.taskCount, loader.ProgressInfo.progress + 1);
            }

            //イベントオブジェクト
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
                    Logger.Error(e);
                }
                loader.ProgressInfo = (loader.ProgressInfo.taskCount, loader.ProgressInfo.progress + 1);
            }

            //イベント
            if (Scene is GameScene gameScene)
            {
                foreach (var item in mapIO.MapEvents)
                {
                    if (GameScene.EndEvents.Any(obj => obj.Key == gameScene.MapPath && obj.Value == item.ID)) continue;
                    try
                    {
                        bool isSkip = false;
                        foreach (var item2 in item.Actors.Where(obj => obj.Path != null && obj.Path.IndexOf(".pc") > -1))
                        {
                            if (!gameScene.CanUsePlayers.Any(obj => obj.Path == item2.Path))
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
                        Logger.Error(e);
                    }
                loader.ProgressInfo = (loader.ProgressInfo.taskCount, loader.ProgressInfo.progress + 1);
                }
            }

            //セーブポイント
            List<SavePoint> tempSavePoints = new List<SavePoint>();
            foreach (var item in mapIO.SavePoints)
            {
                SavePoint savePoint = new SavePoint(item);
                AddObject(savePoint);
                tempSavePoints.Add(savePoint);
                loader.ProgressInfo = (loader.ProgressInfo.taskCount, loader.ProgressInfo.progress + 1);
            }

            //プレイヤー初期配置
            if (initSavePointID != -1 && tempSavePoints.FirstOrDefault(savePoint => savePoint.ID == initSavePointID) != null)
                Player.Position = tempSavePoints.FirstOrDefault(savePoint => savePoint.ID == initSavePointID).Position;
            else if (initDoorID != -1 && tempDoors.FirstOrDefault(door => door.ID == initDoorID) != null)
            {
                Door door = tempDoors.FirstOrDefault(obj => obj.ID == initDoorID);
                Player.Position = door.Position;
                door.Come();
            }

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

            UpdateOtherPlayers();

            if (!SavePoints.Any(obj => obj.IsActive)) 
                PhysicalWorld?.Update();

            UpdateCollision();
        }

        protected override void OnUpdated()
        {
            //イベント開始判定
            if (!MapEvents.Any(obj => obj.IsUpdated))
            {
                foreach (var item in MapEvents)
                {
                    if (Player.CollisionShape.GetIsCollidedWith(item.Shape)) 
                        item.IsUpdated = true;
                }
            }

            UpdateDamage();

            base.OnUpdated();
        }

        void UpdateOtherPlayers()
        {
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
                    if (item2.Value is MapObject.Sensor sensor)
                    {
                        sensor.Update();
                        sensor.Collision = new Collision();
                    }
                }
            }

            //Player<=>MapObject,EventObject
            if (Player?.Collision is Collision playerCollision) playerCollision.ColligingMapObjectTags = Objects.OfType<MapObject>().Where(obj =>
            {
                bool result = Player.CollisionShape.GetIsCollidedWith(obj.CollisionShape);
                if (obj.Collision is Collision collision) collision.IsCollidedWithPlayer = result;
                return result;
            }).Select(obj => obj.Tag).Distinct().ToList();

            //Player=>Obstacle
            if (Player?.Collision is Collision playerCollision2) playerCollision2.IsCollidedWithObstacle = Obstacles.Any(obj => Player.CollisionShape.GetIsCollidedWith(obj));

            //MapObject,EventObject=>Obstacle
            foreach (var item in Objects.OfType<MapObject>())
            {
                if (item.Collision is Collision mapObjectCollision) mapObjectCollision.IsCollidedWithObstacle = Obstacles.Any(obj => obj.GetIsCollidedWith(item.CollisionShape));
            }

            //Sensor=>All
            foreach (var item in Objects.OfType<MapObject>().SelectMany(obj => obj.Sensors).Select(obj => obj.Value).OfType<MapObject.Sensor>())
            {
                if (item.Collision is Collision collision)
                {
                    collision.IsCollidedWithObstacle = Obstacles.Any(obj => item.GetIsCollidedWith(obj));
                    if (Player != null) collision.IsCollidedWithPlayer = item.GetIsCollidedWith((PhysicalShape)Player.CollisionShape);
                    collision.ColligingMapObjectTags = Objects.OfType<MapObject>().Where(obj =>
                        item.GetIsCollidedWith(obj.CollisionShape)).Select(obj => obj.Tag).Distinct().ToList();
                }
            }

            //MapObject=>MapObject
            foreach (var item in Objects.OfType<MapObject>())
            {
                if (item.Collision is Collision collision)
                {
                    collision.ColligingMapObjectTags = Objects.OfType<MapObject>().Where(obj =>
                        obj != item && item.CollisionShape.GetIsCollidedWith(obj.CollisionShape)).Select(obj => obj.Tag).Distinct().ToList();
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

        void OnLeave(Door door)
        {
            if (door.MoveToMap != null && asd.Engine.File.Exists(door.MoveToMap))
            {
                var scene = asd.Engine.CurrentScene as GameScene;
                scene?.ChangeMap
                    (door.MoveToMap,
                    scene.CanUsePlayers,
                    door.IsUseMoveToID ? new asd.Vector2DF() : door.MoveToPosition,
                    door.IsUseMoveToID ? door.MoveToID : -1);
                IsUpdated = false;
            }
            else
            {
                var openDoor = Doors.FirstOrDefault((obj) => obj.ID == door.MoveToID);
                if (openDoor != null)
                {
                    openDoor.Come();
                    Player.Position = door.IsUseMoveToID ? openDoor.Position : door.MoveToPosition;
                }
            }
        }
    }
}
