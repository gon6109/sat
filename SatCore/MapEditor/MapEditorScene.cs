using BaseComponent;
using InspectorModel;
using SatCore.MapEditor.Object;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SatCore.MapEditor
{
    /// <summary>
    /// マップ編集シーン
    /// </summary>
    public class MapEditorScene : BaseEditorScene, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _bGMPath;

        public MapLayer Map { get; }

        [RootPathBinding("root")]
        public string RootPath => Config.Instance.RootPath;

        [TextInput("マップ名")]
        public string MapName
        {
            get => _mapName;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _mapName = value;
                OnPropertyChanged();
            }
        }

        [FileInput("BGM", "WAVE File|*.wav|All File|*.*", "root")]
        public string BGMPath
        {
            get => _bGMPath;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _bGMPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsCanCopy { get; }

        ICopyPasteObject copyObject;
        private string _mapName;

        /// <summary>
        /// コピー
        /// </summary>
        public void Copy()
        {
            UndoRedoManager.Enable = false;
            if (Map.SelectedObject is ICopyPasteObject)
            {
                copyObject = ((ICopyPasteObject)Map.SelectedObject).Copy();
                OnCopyObjectChanged(Map.SelectedObject is ICopyPasteObject, copyObject != null);
            }
            UndoRedoManager.Enable = true;
        }

        /// <summary>
        /// ペースト
        /// </summary>
        public void Paste()
        {
            if (copyObject is BackGround)
            {
                Map.BackGrounds.Add((BackGround)copyObject);
                UndoRedoManager.Enable = false;
                copyObject = copyObject.Copy();
                UndoRedoManager.Enable = true;
            }
            else if (copyObject is asd.Object2D)
            {
                if (copyObject is Door) ((Door)copyObject).ID = Map.GetCanUseDoorID();
                if (copyObject is EventObject) ((EventObject)copyObject).ID = Map.GetCanUseEventObjectID();
                if (copyObject is SavePoint) ((SavePoint)copyObject).ID = Map.GetCanUseSavePointID();
                Map.AddObject((asd.Object2D)copyObject);
                UndoRedoManager.ChangeObject2D(Map, (asd.Object2D)copyObject, true);
                UndoRedoManager.Enable = false;
                copyObject = copyObject.Copy();
                UndoRedoManager.Enable = true;
            }
        }

        public event Action<bool, bool> OnCopyObjectChanged = delegate { };

        public event Action<string, string, INotifyPropertyChanged> OnRequestShowProgressDialog = delegate { };

        [ListInput("Map Objectテンプレート")]
        public ObservableCollection<MapObjectTemplate> MapObjectTemplates { get; set; }

        [SelectedItemBinding("Map Objectテンプレート")]
        public MapObjectTemplate SelectedTemplate { get; set; }

        [AddButtonMethodBinding("Map Objectテンプレート")]
        public void AddTemplate()
        {
            MapObjectTemplates.Add(new MapObjectTemplate());
        }

        [RemoveButtonMethodBinding("Map Objectテンプレート")]
        public void RemoveTemplate(MapObjectTemplate mapObjectTemplate)
        {
            MapObjectTemplates.Remove(mapObjectTemplate);
        }

        [Group("マップヴューア")]
        public MapViewer Viewer { get; set; }

        public MapEditorScene()
        {
            Map = new MapLayer();

            Map.OnChangeSelectedObject += () =>
            {
                OnCopyObjectChanged(Map.SelectedObject is ICopyPasteObject, copyObject != null);
            };
            OnCopyObjectChanged += (isCanCopy, isCanPaste) => { };

            MapObjectTemplates = new ObservableCollection<MapObjectTemplate>();
            try
            {
                var templates = SatIO.BaseIO.Load<SatIO.MapObjectTemplateIO>("mot.data");
                foreach (var item in templates.Templates)
                {
                    var temp = new MapObjectTemplate()
                    {
                        Name = item.Key,
                        ScriptPath = item.Value.ScriptPath
                    };
                    MapObjectTemplates.Add(temp);
                }
            }
            catch
            {
            }

            Viewer = new MapViewer(this);

            AddLayer(Map);
        }

        protected override void OnUpdated()
        {
            if (Input.GetInputState(Inputs.Esc) == 1)
                asd.Engine.Reload();

            if (Viewer.IsRequireProgressDialog)
            {
                OnRequestShowProgressDialog("Loading Map", "Progress", Viewer);
                Viewer.IsRequireProgressDialog = false;
            }

            base.OnUpdated();
        }

        protected override void OnDispose()
        {
            OnRequestShowProgressDialog = delegate { };
            OnCopyObjectChanged = delegate { };
            SatIO.MapObjectTemplateIO templateIO = new SatIO.MapObjectTemplateIO();
            foreach (var item in MapObjectTemplates)
            {
                var temp = new SatIO.EventObjectIO()
                {
                    ScriptPath = item.ScriptPath
                };
                templateIO.Templates.Add(item.Name, temp);
            }
            templateIO.Save("mot.data");
            base.OnDispose();
        }

        public async Task LoadMapData(string path)
        {
            Path = path;
            try
            {
                Debug.PrintCount("Update");

                Debug.PrintTime();
                var mapData = await SatIO.BaseIO.LoadAsync<SatIO.MapIO>(path);
                MapName = mapData.MapName;
                Logger.Debug("File Loaded");
                Debug.PrintCount("Update");
                Debug.PrintTime();

                await Map.LoadMapDataAsync(mapData);
                BGMPath = mapData.BGMPath;
                Debug.PrintTime();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override void SaveImp(string path)
        {
            base.SaveImp(path);
            var mapdata = new SatIO.MapIO()
            {
                BGMPath = BGMPath,
                MapName = MapName,
            };
            Map.SaveMapData(mapdata);
            mapdata.Save(path);
        }

        /// <summary>
        /// マップビューアー
        /// </summary>
        public class MapViewer : INotifyPropertyChanged, ILoader
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private (int taskCount, int progress) progressInfo;
            private bool isCancel;

            [ListInput("プレイヤー")]
            public ObservableCollection<PlayerName> PlayerNames { get; set; }

            [AddButtonMethodBinding("プレイヤー")]
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

            [RemoveButtonMethodBinding("プレイヤー")]
            public void RemovePlayerData(PlayerName playerName)
            {
                PlayerNames.Remove(playerName);
            }

            [VectorInput("初期座標")]
            public asd.Vector2DF PlayerPosition { get; set; }

            public MapEditorScene RefMapEditor { get; private set; }
            public (int taskCount, int progress) ProgressInfo
            {
                get => progressInfo;
                set
                {
                    progressInfo = value;
                    OnPropertyChanged("Progress");
                }
            }

            public bool IsRequireProgressDialog { get; set; }

            public bool IsCancel
            {
                get => isCancel;
                set
                {
                    isCancel = value;
                    OnPropertyChanged();
                }
            }

            public int Progress => ProgressInfo.taskCount != 0 ? (int)((float)ProgressInfo.progress / ProgressInfo.taskCount * 100) : 0;

            public MapViewer(MapEditorScene mapEditor)
            {
                RefMapEditor = mapEditor;
                PlayerNames = new ObservableCollection<PlayerName>();
            }

            [Button("Play")]
            public async Task PlayMapAsync()
            {
                try
                {
                    if (PlayerNames.Count == 0 || asd.Engine.CurrentScene != RefMapEditor) return;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    IsCancel = false;
                    IsRequireProgressDialog = true;

                    RefMapEditor.SaveImp("temp.map");
                    await SatPlayer.Game.GameScene.LoadPlayersDataAsync();
                    var newScene = new SatPlayer.Game.GameScene("temp.map", SatPlayer.Game.GameScene.Players.Where(obj => PlayerNames.Any(obj2 => obj.Path == obj2.Name)).ToList(), PlayerPosition, isPreviewMode: true);
                    newScene.OnGameOver += (() =>
                    {
                        SatPlayer.Game.GameScene.EndEvents.Clear();
                        asd.Engine.ChangeScene(RefMapEditor);
                    });
                    newScene.OnChangeMap += (path, initPlayers, playerPosition, doorID, savePointID) =>
                    {
                        SatPlayer.Game.GameScene.EndEvents.Clear();
                        asd.Engine.ChangeScene(RefMapEditor);
                    };
                    newScene.OnEnd += () =>
                    {
                        SatPlayer.Game.GameScene.EndEvents.Clear();
                        asd.Engine.ChangeScene(RefMapEditor);
                    };

                    await newScene.LoadMapAsync(this);
                    ProgressInfo = default;
                    IsCancel = true;

                    asd.Engine.ChangeScene(newScene, false);
                }
                catch (Exception e)
                {
                    IsRequireProgressDialog = false;
                    isCancel = true;
                    Logger.Error(e);
                }
            }

            public class PlayerName : IListInput
            {
                public string Name { get; set; }
            }
        }
    }
}
