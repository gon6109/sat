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
using SatPlayer.Game.Object;

namespace SatPlayer.Game
{
    public delegate void ChangeMapEvent(string path, List<Player> initPlayers, asd.Vector2DF playerPosition, int doorID = -1, int savePointID = -1);
    
    /// <summary>
    /// ゲームシーン
    /// </summary>
    public class GameScene : asd.Scene
    {
        public static List<Player> Players { get; } = new List<Player>();
        public static List<KeyValuePair<string, int>> EndEvents { get; } = new List<KeyValuePair<string, int>>();

        public event ChangeMapEvent OnChangeMapEvent = delegate { };
        public event Action OnGameOver = delegate { };
        public event Action OnEnd = delegate { };

        public bool IsPreviewMode { get; }

        public string MapName { get; private set; }

        MapLayer Map { get; }

        public List<Player> CanUsePlayers { get; set; }

        public string MapPath { get; }
        int InitDoorID { get; }
        int InitSavePointID { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">マップへのパス</param>
        /// <param name="initPlayers">使用プレイヤーたち</param>
        /// <param name="playerPosition">プレイヤーの初期座標</param>
        /// <param name="doorID">初期ドアID</param>
        /// <param name="savePointID">初期セーブポイントID</param>
        /// <param name="isPreviewMode">プレビューモードか</param>
        public GameScene(string path, List<Player> initPlayers, asd.Vector2DF playerPosition, int doorID = -1, int savePointID = -1, bool isPreviewMode = false)
        {
            CanUsePlayers = initPlayers;
            foreach (var item in CanUsePlayers)
            {
                item.Position = playerPosition;
            }
            MapPath = path;

            //レイヤー登録
            {
                Map = new MapLayer(CanUsePlayers[0]);
            }

            InitDoorID = doorID;
            InitSavePointID = savePointID;
            IsPreviewMode = isPreviewMode;
        }

        public IEnumerator<int> Init()
        {
            AddLayer(Map);
            MessageLayer2D.Reset();
            AddLayer(MessageLayer2D.Instance);


            MapIO mapIO = new MapIO();
            mapIO = BaseIO.Load<MapIO>(MapPath);
            MapName = mapIO.MapName;

            var enumerator = Map.LoadMapData( mapIO, InitDoorID, InitSavePointID);

            foreach (var item in CanUsePlayers)
            {
                item.CollisionShape = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, Map.PhysicalWorld);
                item.CollisionShape.GroupIndex = -1;
                Map.AddObject(item);
            }

            Sound.StartBgm(new Sound(mapIO.BGMPath), 2);
        }

        public async Task CreateMapAsync()
        {
            AddLayer(Map);
            MessageLayer2D.Reset();
            AddLayer(MessageLayer2D.Instance);


            MapIO mapIO = new MapIO();
            mapIO = BaseIO.Load<MapIO>(MapPath);
            MapName = mapIO.MapName;
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
            OnGameOver = delegate { };
            OnEnd = delegate { };
            OnChangeMapEvent = (iPath, iInitPlayers, iPlayerPosition, iDoorID, isavePointID) => { };
            base.OnStopUpdating();
        }

        bool isMessageClose = false;

        protected override void OnUpdating()
        {
            base.OnUpdating();
            if (MessageLayer2D.Count != 0)
            {
                Map.Player.CollisionShape.IsActive = false;
                Map.Player.IsUpdated = false;
                isMessageClose = true;
            }
            else if (isMessageClose)
            {
                isMessageClose = false;
                Map.Player.CollisionShape.IsActive = true;
                Map.Player.IsUpdated = true;
            }
        }

        protected override void OnUpdated()
        {
            if (IsPreviewMode && Input.GetInputState(Inputs.Esc) > 0) OnGameOver();

            base.OnUpdated();
        }

        protected override void OnUnregistered()
        {
            OnGameOver = delegate { };
            OnChangeMapEvent = delegate { };
            OnEnd = delegate { };
            foreach (var item in CanUsePlayers)
            {
                Map.RemoveObject(item);
            }
            base.OnUnregistered();
        }

        public SaveDataIO ToSaveData()
        {
            return new SaveDataIO()
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
