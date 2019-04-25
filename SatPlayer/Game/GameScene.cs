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
using System.Xml.Serialization;

namespace SatPlayer.Game
{
    public delegate void ChangeMapEvent(string path, List<Player> initPlayers, asd.Vector2DF playerPosition, int doorID = -1, int savePointID = -1);

    /// <summary>
    /// ゲームシーン
    /// </summary>
    public class GameScene : asd.Scene
    {
        /// <summary>
        /// 登録されたプレイヤー
        /// </summary>
        public static List<Player> Players { get; } = new List<Player>();

        /// <summary>
        /// 終了済みイベント
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<string, int>> EndEvents { get; } = new List<KeyValuePair<string, int>>();

        /// <summary>
        /// マップ遷移時イベント
        /// </summary>
        public event ChangeMapEvent OnChangeMap = delegate { };

        /// <summary>
        /// ゲームオーバー時イベント
        /// </summary>
        public event Action OnGameOver = delegate { };

        /// <summary>
        /// 終了時イベント
        /// </summary>
        public event Action OnEnd = delegate { };

        /// <summary>
        /// プレビューモードか
        /// </summary>
        public bool IsPreviewMode { get; }

        /// <summary>
        /// マップ名
        /// </summary>
        public string MapName { get; private set; }

        /// <summary>
        /// マップレイヤー
        /// </summary>
        MapLayer Map { get; }

        /// <summary>
        /// マップで使用できるプレイヤー
        /// </summary>
        public List<Player> CanUsePlayers { get; set; }

        /// <summary>
        /// マップファイルのパス
        /// </summary>
        /// <value></value>
        public string MapPath { get; }

        /// <summary>
        /// 初期ドアID
        /// </summary>
        int InitDoorID { get; }

        /// <summary>
        /// 初期セーブポイント
        /// </summary>
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

        /// <summary>
        /// マップをロードする
        /// </summary>
        /// <param name="info">ロード情報</param>
        /// <returns>タスク</returns>
        public async Task LoadMapAsync((int taskCount, int progress) info)
        {
            info.progress = 0;

            AddLayer(Map);
            MessageLayer2D.Reset();
            AddLayer(MessageLayer2D.Instance);

            MapIO mapIO = new MapIO();
            mapIO = await BaseIO.LoadAsync<MapIO>(MapPath);
            MapName = mapIO.MapName;

            info.taskCount = mapIO.GetMapElementCount();

            await Map.LoadMapData(mapIO, InitDoorID, InitSavePointID, info);

            foreach (var item in CanUsePlayers)
            {
                Map.AddObject(item);
            }

            Sound.StartBgm(new Sound(mapIO.BGMPath), 2);
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
            OnChangeMap = delegate { };
            base.OnStopUpdating();
        }

        protected override void OnUpdating()
        {
            base.OnUpdating();
        }

        protected override void OnUpdated()
        {
            //プレビュー時エスケープで終了
            if (IsPreviewMode && Input.GetInputState(Inputs.Esc) > 0) OnEnd();

            base.OnUpdated();
        }

        protected override void OnUnregistered()
        {
            OnGameOver = delegate { };
            OnChangeMap = delegate { };
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
        public static async Task LoadPlayersDataAsync()
        {
            Players.Clear();
            XmlSerializer serializser = new XmlSerializer(typeof(List<string>));
            using (var stream = await IO.GetStreamAsync("Player/PlayersList.dat"))
            {
                var playerDataPaths = (List<string>)serializser.Deserialize(stream);
                foreach (var item in playerDataPaths)
                {
                    if (!asd.Engine.File.Exists(item)) continue;
                    Player player = await Player.CreatePlayerAsync(item);
                    Players.Add(player);
                }
            }
        }

        /// <summary>
        /// マップを遷移する
        /// </summary>
        /// <param name="path">遷移先マップファイルのパス</param>
        /// <param name="initPlayers">使用可能プレイヤー</param>
        /// <param name="playerPosition">プレイヤー初期座標</param>
        /// <param name="doorID">初期ドアID</param>
        /// <param name="savePointID">初期セーブポイント</param>
        public void ChangeMap(string path, List<Player> initPlayers, asd.Vector2DF playerPosition, int doorID = -1, int savePointID = -1)
        {
            OnChangeMap(path, initPlayers, playerPosition, doorID, savePointID);
        }

        /// <summary>
        /// 終了する
        /// </summary>
        public void End()
        {
            OnEnd();
        }

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        public void GameOver()
        {
            OnGameOver();
        }
    }
}
