using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BaseComponent;
using InspectorModel;
using SatCore.MapEditor.Object;

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

        [ListInput("背景", selectedObjectBindingPath: "SelectedBackGround", addButtonEventMethodName: "AddBackGround")]
        public UndoRedoCollection<BackGround> BackGrounds { get; }

        bool isSelectedBackGround;

        /// <summary>
        /// 選択されている背景
        /// </summary>
        public BackGround SelectedBackGround
        {
            get => _selectedBackGround;
            set
            {
                isSelectedBackGround = true;
                OnCopyObjectChanged(true, copyObject != null);
                _selectedBackGround = value;
            }
        }

        public void AddBackGround()
        {
            BackGrounds.Add(new BackGround());
        }

        [FileInput("BGM", "WAVE File|*.wav|All File|*.*")]
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
        private BackGround _selectedBackGround;
        private string _mapName;

        /// <summary>
        /// コピー
        /// </summary>
        public void Copy()
        {
            UndoRedoManager.Enable = false;
            if (isSelectedBackGround)
            {
                copyObject = SelectedBackGround.Copy();
                OnCopyObjectChanged(Map.SelectedObject is ICopyPasteObject, copyObject != null);
            }
            else if (Map.SelectedObject is ICopyPasteObject)
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
                BackGrounds.Add((BackGround)copyObject);
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

        public event Action<string, string, INotifyPropertyChanged
            > OnRequestShowProgressDialog = delegate { };

        [ListInput("Map Objectテンプレート", "SelectedTemplate", "AddTemplate")]
        public ObservableCollection<MapObjectTemplate> MapObjectTemplates { get; set; }

        public MapObjectTemplate SelectedTemplate { get; set; }

        public void AddTemplate()
        {
            MapObjectTemplates.Add(new MapObjectTemplate());
        }

        [Group("マップヴューア")]
        public MapViewer Viewer { get; set; }
        public bool IsRemoveBackGround { get; private set; }

        public MapEditorScene()
        {
            Map = new MapLayer();

            BackGrounds = new UndoRedoCollection<BackGround>();
            BackGrounds.CollectionChanged += BackGrounds_CollectionChanged;

            Map.OnChangeSelectedObject += () =>
            {
                isSelectedBackGround = false;
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

        /// <summary>
        /// 背景更新時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackGrounds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Map.AddObject((BackGround)e.NewItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    ((BackGround)e.OldItems[0]).Dispose();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    RemoveBackGround();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    RemoveBackGround();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    RemoveBackGround();
                    break;
                default:
                    break;
            }
        }

        void RemoveBackGround()
        {
            foreach (var item in Map.Objects.OfType<BackGround>())
            {
                Map.RemoveObject(item);
            }
            IsRemoveBackGround = true;
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

            if (IsRemoveBackGround)
            {
                foreach (var item in BackGrounds)
                    Map.AddObject(item);
                IsRemoveBackGround = false;
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
                var mapdata = await SatIO.BaseIO.LoadAsync<SatIO.MapIO>(path);
                MapName = mapdata.MapName;
                Logger.Debug("File Loaded");
                Debug.PrintCount("Update");
                Debug.PrintTime();

                if (mapdata.BackGrounds != null)
                {
                    foreach (var item in mapdata.BackGrounds)
                    {
                        Debug.SetFlag("Timer", true);
                        BackGrounds.Add(await BackGround.CreateBackGroudAsync(item));
                        Debug.SetFlag("Timer", false);
                        Logger.Debug("BackGround Loaded");
                        Debug.PrintCount("Update");
                        Debug.PrintTime();
                    }
                }

                await Map.LoadMapDataAsync(mapdata);
                BGMPath = mapdata.BGMPath;
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
                BackGrounds = BackGrounds.Select(obj => obj.ToIO()).ToList(),
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

            [ListInput("プレイヤー", addButtonEventMethodName: "AddPlayerData")]
            public ObservableCollection<PlayerName> PlayerNames { get; set; }

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
