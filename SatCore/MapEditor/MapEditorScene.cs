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
using SatCore.Attribute;
using SatCore.MapEditor.Object;

namespace SatCore.MapEditor
{
    /// <summary>
    /// マップ編集シーン
    /// </summary>
    public class MapEditorScene : UndoRedoScene, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _bGMPath;
        private IEnumerator<int> cameraUpdater;

        public MapLayer Map { get; }

        public string Path { get; set; }

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

        [ListInput("背景", selectedObjectBindingPath: "SelectedBackGround", additionButtonEventMethodName: "AddBackGround")]
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

        [ListInput("Map Objectテンプレート", "SelectedTemplate", "AddTemplate")]
        public ObservableCollection<MapObjectTemplate> MapObjectTemplates { get; set; }

        public MapObjectTemplate SelectedTemplate { get; set; }

        public void AddTemplate()
        {
            MapObjectTemplates.Add(new MapObjectTemplate());
        }

        [Group("マップヴューア")]
        public MapViewer Viewer { get; set; }

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
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
            cameraUpdater = UpdateCamera();
        }

        IEnumerator<int> UpdateCamera()
        {
            foreach (var item in BackGrounds)
            {
                item.Camera.Layer?.RemoveObject(item.Camera);
            }
            yield return 0;
            foreach (var item in BackGrounds)
            {
                Map.AddObject(item.Camera);
            }
            yield return 0;
        }

        protected override void OnUpdated()
        {
            cameraUpdater?.MoveNext();
            if (Input.GetInputState(Inputs.Esc) == 1)
                asd.Engine.Reload();
            base.OnUpdated();
        }

        protected override void OnDispose()
        {
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
            if (Path != null) ErrorIO.SaveError(Path.Split('.')[0] + ".log");
            base.OnDispose();
        }

        public void LoadMapData(string path)
        {
            Path = path;
            try
            {
                var mapdata = SatIO.BaseIO.Load<SatIO.MapIO>(path);
                MapName = mapdata.MapName;
                if (mapdata.BackGrounds != null)
                {
                    foreach (var item in mapdata.BackGrounds)
                    {
                        BackGrounds.Add(BackGround.CreateBackGroud(item));
                    }
                }
                _ = Map.LoadMapDataAsync(mapdata);
                BGMPath = mapdata.BGMPath;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SaveMapData(string path)
        {
            var mapdata = new SatIO.MapIO()
            {
                BGMPath = BGMPath,
                Path = Path,
                BackGrounds = BackGrounds.Select(obj => obj.ToIO()).ToList(),
                MapName = MapName,
            };
            Map.SaveMapData(mapdata);
            mapdata.Save(path);
        }

        /// <summary>
        /// マップビューアー
        /// </summary>
        public class MapViewer
        {

            [ListInput("プレイヤー", additionButtonEventMethodName: "AddPlayerData")]
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

            bool isLoading;

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
                    if (isLoading || PlayerNames.Count == 0 || asd.Engine.CurrentScene != RefMapEditor) return;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    RefMapEditor.SaveMapData("temp.map");
                    await SatPlayer.Game.GameScene.LoadPlayersDataAsync();
                    var newScene = new SatPlayer.Game.GameScene("temp.map", SatPlayer.Game.GameScene.Players.Where(obj => PlayerNames.Any(obj2 => obj.Path == obj2.Name)).ToList(), PlayerPosition, isPreviewMode: true);
                    newScene.OnGameOver += (() =>
                    {
                        asd.Engine.ChangeScene(RefMapEditor);
                    });
                    newScene.OnChangeMap += (path, initPlayers, playerPosition, doorID, savePointID) =>
                    {
                        asd.Engine.ChangeScene(RefMapEditor);
                    };
                    newScene.OnEnd += () =>
                    {
                        asd.Engine.ChangeScene(RefMapEditor);
                    };
                    (int taskCount, int progress) info = default;
                    await newScene.LoadMapAsync(info);
                    asd.Engine.ChangeScene(newScene, false);
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
            }

            public class PlayerName : IListInput
            {
                public string Name { get; set; }
            }
        }
    }
}
