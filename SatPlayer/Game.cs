using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using BaseComponent;
using PhysicAltseed;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Concurrent;
using SatIO;

namespace SatPlayer
{
    public delegate void ChangeMapEvent(string path, List<Player> initPlayers, asd.Vector2DF playerPosition, int doorID = -1, int savePointID = -1);

    public class Game : asd.Scene
    {
        public static List<Player> Players { get; private set; } = new List<Player>();
        public static List<KeyValuePair<string, int>> EndEvents { get; private set; } = new List<KeyValuePair<string, int>>();

        public ChangeMapEvent OnChangeMapEvent { get; set; }
        public Action OnGameOver { get; set; }
        public Action OnEnd { get; set; }
        void DefaulFunc() { }

        public bool IsPreviewMode { get; set; }

        public string MapName { get; private set; }

        MainMapLayer2D mainLayer;

        public List<Player> CanUsePlayers { get; set; }

        public int ElementCount
        {
            get => mainLayer != null ? mainLayer.ElementCount : 0;
            private set
            {
                if (mainLayer != null) mainLayer.ElementCount = value;
            }
        }

        public int LoadingElementCount
        {
            get => mainLayer != null ? mainLayer.LoadingElementCount : 0;
            private set
            {
                if (mainLayer != null) mainLayer.LoadingElementCount = value;
            }
        }

        public string MapPath { get; private set; }

        int initDoorID;
        int initSavePointID;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">マップへのパス</param>
        /// <param name="playerPosition">プレイヤーの初期座標</param>
        public Game(string path, List<Player> initPlayers, asd.Vector2DF playerPosition, int doorID = -1, int savePointID = -1, bool isPreviewMode = false)
        {
            CanUsePlayers = initPlayers;
            foreach (var item in CanUsePlayers)
            {
                item.Position = playerPosition;
            }
            MapPath = path;

            //レイヤー登録
            {
                mainLayer = new MainMapLayer2D(CanUsePlayers[0]);
            }

            OnGameOver = DefaulFunc;
            OnEnd = DefaulFunc;
            OnChangeMapEvent = (iPath, iInitPlayers, iPlayerPosition, iDoorID, isavePointID) => { };
            initDoorID = doorID;
            initSavePointID = savePointID;
            IsPreviewMode = isPreviewMode;
        }

        BlockingCollection<Action> subThreadTasks;
        bool isFin;

        public IEnumerator<int> Init()
        {
            AddLayer(mainLayer);
            MessageLayer2D.Reset();
            AddLayer(MessageLayer2D.Instance);

            subThreadTasks = new BlockingCollection<Action>();
            BlockingCollection<Action> mainThreadTasks = new BlockingCollection<Action>();
            isFin = false;
            var task = SubInit();

            SatIO.BinaryMapIO mapIO = new SatIO.BinaryMapIO();
            mapIO = SatIO.BinaryMapIO.LoadMap(MapPath);
            MapName = mapIO.MapName;

            ElementCount = mapIO.BackGrounds.Count + mapIO.Doors.Count + mapIO.MapObjects.Count + mapIO.NPCMapObjects.Count + mapIO.MapEvents.Count;

            var enumerator = mainLayer.LoadMapData(subThreadTasks, mainThreadTasks, mapIO, initDoorID, initSavePointID);
            while (enumerator.MoveNext()) yield return 0;
            isFin = true;

            foreach (var item in CanUsePlayers)
            {
                item.CollisionShape = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, mainLayer.PhysicalWorld);
                item.CollisionShape.GroupIndex = -1;
                mainLayer.AddObject(item);
            }

            Sound.StartBgm(new Sound(mapIO.BGMPath), 2);

            while (!task.IsCompleted || mainThreadTasks.Count != 0)
            {
                Action action;
                mainThreadTasks.TryTake(out action, 100);
                action?.Invoke();
                yield return 0;
            }
        }

        async Task SubInit()
        {
            await Task.Run(() => RunSubThread(subThreadTasks, ref isFin));
        }

        void RunSubThread(BlockingCollection<Action> queue, ref bool isFinish)
        {
            while (!isFinish || queue.Count != 0)
            {
                Action task;
                queue.TryTake(out task, 100);
                task?.Invoke();
            }
        }

        protected override void OnStartUpdating()
        {
            Sound.StopBgm(0.5f);
            base.OnStartUpdating();
        }

        protected override void OnTransitionBegin()
        {
            base.OnTransitionBegin();
        }

        protected override void OnStopUpdating()
        {
            asd.Engine.Sound.StopAll();
            OnGameOver = DefaulFunc;
            OnEnd = DefaulFunc;
            OnChangeMapEvent = (iPath, iInitPlayers, iPlayerPosition, iDoorID, isavePointID) => { };
            base.OnStopUpdating();
        }

        bool isMessageClose = false;

        protected override void OnUpdating()
        {
            base.OnUpdating();
            if (MessageLayer2D.Count != 0)
            {
                mainLayer.Player.CollisionShape.IsActive = false;
                mainLayer.Player.IsUpdated = false;
                isMessageClose = true;
            }
            else if (isMessageClose)
            {
                isMessageClose = false;
                mainLayer.Player.CollisionShape.IsActive = true;
                mainLayer.Player.IsUpdated = true;
            }
        }

        protected override void OnUpdated()
        {
            if (IsPreviewMode && Input.GetInputState(Inputs.Esc) > 0) OnGameOver();

            base.OnUpdated();
        }

        protected override void OnUnregistered()
        {
            OnGameOver = DefaulFunc;
            OnChangeMapEvent = (iPath, iInitPlayers, iPlayerPosition, iDoorID, iSavePointID) => { };
            OnEnd = DefaulFunc;
            foreach (var item in CanUsePlayers)
            {
                mainLayer.RemoveObject(item);
            }
            base.OnUnregistered();
        }

        public SaveSatIO ToSaveData()
        {
            return new SaveSatIO()
            {
                EndEvents = EndEvents,
                MapName = MapName,
                MapPath = MapPath,
                PlayingChacacter = CanUsePlayers.Select(obj => obj.Name).ToList(),
                Time = 0, //TODO : 時間
            };
        }

        /// <summary>
        /// プレイヤーデータの読み込み
        /// </summary>
        public static void LoadPlayersData()
        {
            Players.Clear();
            BinaryFormatter serializser = new BinaryFormatter();
            var playerDataPaths = (List<string>)serializser.Deserialize(IO.GetStream("Player/PlayersList.dat"));
            foreach (var item in playerDataPaths)
            {
                if (!asd.Engine.File.Exists(item)) continue;
                Player player = new Player(item);
                Players.Add(player);
            }
        }
    }
}
