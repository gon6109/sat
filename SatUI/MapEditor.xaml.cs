using BaseComponent;
using AltseedInspector;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using InspectorModel;

namespace SatUI
{
    /// <summary>
    /// MapEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class MapEditor : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Dictionary<Inputs, asd.ButtonState> InputData { get; set; }

        [DirectoryInput("ルートディレクトリ")]
        public string RootPath
        {
            get => SatCore.Config.Instance.RootPath;
            set
            {
                if (asd.Engine.File == null || value == null) return;

                asd.Engine.File.ClearRootDirectories();
                try
                {
                    asd.Engine.File.AddRootDirectory(System.IO.Path.GetDirectoryName(value));
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

                SatCore.Config.Instance.RootPath = value;

                SatCore.PlayersListDialog.CheckPlayersList(value);
                OnPropertyChanged();
            }
        }

        public MapEditor()
        {
            InitializeComponent();
            EditorPanel.Child = new System.Windows.Forms.Control();
            EditorPanel.PreviewKeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.Up ||
                e.Key == Key.Down ||
                e.Key == Key.Left ||
                e.Key == Key.Right ||
                e.Key == Key.Enter ||
                e.Key == Key.Tab) e.Handled = true;
            };
            Property mapProperty = new Property("General", this);

            inspector.AddProperty(mapProperty);
            SatCore.UndoRedoManager.OnUpdateData += OnUpdateUndoRedoData;
            gridSplitter.IsEnabled = false;
            codeColumn.Width = new GridLength(0);
        }

        private void map_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene is SatCore.BaseEditorScene scene)
                if (scene.ConfirmSave() == SatCore.BaseEditorScene.ConfirmSaveDialogResult.Cancel)
                    return;
                else
                    scene.RemoveEvent();

            var newFile = new SatCore.MapEditor.MapEditorScene();
            newFile.OnRequestShowProgressDialog += ShowProgressDialog;
            newFile.Map.OnChangeSelectedObject += OnChangeSelectedObject;
            newFile.Map.OnCreateDoor += OnCreateDoor;
            newFile.Map.OnCreateMapObject += OnCreateMapObject;
            newFile.Map.FocusToEditorPanel += () => EditorPanel.Focus();
            newFile.Map.RequireOpenFileDialog += OpenCharacterImageFileDialog;
            newFile.OnCopyObjectChanged += OnCopyObjectChanged;
            newFile.OnSave += SaveMap;
            newFile.RequireConfirmSaveDialog += OpenConfirmSaveDialog;
            asd.Engine.ChangeScene(newFile);

            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General");
            Property mapProperty = new Property("Map", new object[] { newFile.Map, newFile });
            inspector.AddProperty(mapProperty);
        }

        private void ShowProgressDialog(string title, string bindingPath, INotifyPropertyChanged bindingSource)
        {
            var dialog = new ProgressDialog(title, bindingPath, bindingSource);
            dialog.Owner = GetWindow(this);
            dialog.Show();
        }

        private void characterImage_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene is SatCore.BaseEditorScene scene)
                if (scene.ConfirmSave() == SatCore.BaseEditorScene.ConfirmSaveDialogResult.Cancel)
                    return;
                else
                    scene.RemoveEvent();

            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General");

            var loadFile = new SatCore.CharacterImageEditor.CharacterImageEditor();
            loadFile.OnSave += SaveCharacterImage;
            loadFile.RequireConfirmSaveDialog += OpenConfirmSaveDialog;
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Chracter Image", loadFile.Character);
            inspector.AddProperty(mapProperty);
        }

        private void CreateMapObject_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene is SatCore.BaseEditorScene scene)
                if (scene.ConfirmSave() == SatCore.BaseEditorScene.ConfirmSaveDialogResult.Cancel)
                    return;
                else
                    scene.RemoveEvent();

            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General");

            var loadFile = new SatCore.ScriptEditor.ScriptEditor(SatCore.ScriptEditor.ScriptEditor.ScriptType.MapObject);
            loadFile.OnSave += SaveScript;
            loadFile.RequireConfirmSaveDialog += OpenConfirmSaveDialog;
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Map Object", new object[] { loadFile, loadFile.ScriptObject });
            inspector.AddProperty(mapProperty);
            code.Children.Add(new CodeEditor(loadFile.ScriptObject, "Code"));
            gridSplitter.IsEnabled = true;
            codeColumn.Width = new GridLength(EditorPanel.ActualWidth / 2);
        }

        private void CreateEventObject_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene is SatCore.BaseEditorScene scene)
                if (scene.ConfirmSave() == SatCore.BaseEditorScene.ConfirmSaveDialogResult.Cancel)
                    return;
                else
                    scene.RemoveEvent();

            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General");

            var loadFile = new SatCore.ScriptEditor.ScriptEditor(SatCore.ScriptEditor.ScriptEditor.ScriptType.EventObject);
            loadFile.OnSave += SaveScript;
            loadFile.RequireConfirmSaveDialog += OpenConfirmSaveDialog;
            asd.Engine.ChangeScene(loadFile);

            Property mapProperty = new Property("Map Object", new object[] { loadFile, loadFile.ScriptObject });
            inspector.AddProperty(mapProperty);
            code.Children.Add(new CodeEditor(loadFile.ScriptObject, "Code"));
            gridSplitter.IsEnabled = true;
            codeColumn.Width = new GridLength(EditorPanel.ActualWidth / 2);
        }

        private void CreatePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene is SatCore.BaseEditorScene scene)
                if (scene.ConfirmSave() == SatCore.BaseEditorScene.ConfirmSaveDialogResult.Cancel)
                    return;
                else
                    scene.RemoveEvent();

            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General");

            var loadFile = new SatCore.ScriptEditor.ScriptEditor(SatCore.ScriptEditor.ScriptEditor.ScriptType.Player);
            loadFile.OnSave += SaveScript;
            loadFile.RequireConfirmSaveDialog += OpenConfirmSaveDialog;
            asd.Engine.ChangeScene(loadFile);

            Property mapProperty = new Property("Map Object", new object[] { loadFile, loadFile.ScriptObject });
            inspector.AddProperty(mapProperty);
            code.Children.Add(new CodeEditor(loadFile.ScriptObject, "Code"));
            gridSplitter.IsEnabled = true;
            codeColumn.Width = new GridLength(EditorPanel.ActualWidth / 2);
        }

        private void CreateBackGround_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene is SatCore.BaseEditorScene scene)
                if (scene.ConfirmSave() == SatCore.BaseEditorScene.ConfirmSaveDialogResult.Cancel)
                    return;
                else
                    scene.RemoveEvent();

            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General");

            var loadFile = new SatCore.ScriptEditor.ScriptEditor(SatCore.ScriptEditor.ScriptEditor.ScriptType.BackGround);
            loadFile.OnSave += SaveScript;
            loadFile.RequireConfirmSaveDialog += OpenConfirmSaveDialog;
            asd.Engine.ChangeScene(loadFile);

            Property mapProperty = new Property("Map Object", new object[] { loadFile, loadFile.ScriptObject });
            inspector.AddProperty(mapProperty);
            code.Children.Add(new CodeEditor(loadFile.ScriptObject, "Code"));
            gridSplitter.IsEnabled = true;
            codeColumn.Width = new GridLength(EditorPanel.ActualWidth / 2);
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene is SatCore.BaseEditorScene scene)
                if (scene.ConfirmSave() == SatCore.BaseEditorScene.ConfirmSaveDialogResult.Cancel)
                    return;
                else
                    scene.RemoveEvent();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "";
            openFileDialog.InitialDirectory = "";
            openFileDialog.Filter = "All Readable File|*.map;*.pc;*.ci;*.bg;*.mobj;*.eobj" +
                "|Map File|*.map|Player Script|*.pc|Character Image File|*.ci|Map Object Script|*.mobj|Event Object Script|*.eobj|Back Ground Script|*.bg|All File|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != true) return;

            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General");

            if (openFileDialog.FileName.Contains(".map")) await OpenMapFile(openFileDialog.FileName);
            else if (openFileDialog.FileName.Contains(".ci")) OpenCharacterImageFile(openFileDialog.FileName);
            else OpenScriptFile(openFileDialog.FileName);
        }

        async Task OpenMapFile(string fileName)
        {
            try
            {
                var loadFile = new SatCore.MapEditor.MapEditorScene();
                loadFile.OnRequestShowProgressDialog += ShowProgressDialog;
                loadFile.Map.OnChangeSelectedObject += OnChangeSelectedObject;
                loadFile.Map.OnCreateDoor += OnCreateDoor;
                loadFile.Map.OnCreateMapObject += OnCreateMapObject;
                loadFile.Map.FocusToEditorPanel += () => EditorPanel.Focus();
                loadFile.Map.RequireOpenFileDialog += OpenCharacterImageFileDialog;
                loadFile.OnCopyObjectChanged += OnCopyObjectChanged;
                loadFile.OnSave += SaveMap;
                loadFile.RequireConfirmSaveDialog += OpenConfirmSaveDialog;
                await loadFile.LoadMapData(fileName);
                asd.Engine.ChangeScene(loadFile);

                Property mapProperty = new Property("Map", new object[] { loadFile.Map, loadFile });
                inspector.AddProperty(mapProperty);
            }
            catch (Exception e)
            {
                var printer = new LogPrintLayer2D();
                printer.DrawingPriority = 3;
                Logger.Printer = printer;
                asd.Engine.CurrentScene.AddLayer(printer);
                Logger.Error(new Exception(fileName + "の読み込みに失敗しました(" + e.GetType().ToString() + ")"));
            }
        }

        void OpenCharacterImageFile(string fileName)
        {
            var loadFile = new SatCore.CharacterImageEditor.CharacterImageEditor(fileName);
            loadFile.OnSave += SaveCharacterImage;
            loadFile.RequireConfirmSaveDialog += OpenConfirmSaveDialog;
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Chracter Image", loadFile.Character);
            inspector.AddProperty(mapProperty);
        }

        void OpenScriptFile(string fileName)
        {
            SatCore.ScriptEditor.ScriptEditor.ScriptType scriptType = new SatCore.ScriptEditor.ScriptEditor.ScriptType();

            if (fileName.Contains(".mobj")) scriptType = SatCore.ScriptEditor.ScriptEditor.ScriptType.MapObject;
            else if (fileName.Contains(".eobj")) scriptType = SatCore.ScriptEditor.ScriptEditor.ScriptType.EventObject;
            else if (fileName.Contains(".pc")) scriptType = SatCore.ScriptEditor.ScriptEditor.ScriptType.Player;
            else if (fileName.Contains(".bg")) scriptType = SatCore.ScriptEditor.ScriptEditor.ScriptType.BackGround;
            else
            {
                Logger.Error(new NotImplementedException("対応していない拡張子です。"));
                return;
            }

            var loadFile = new SatCore.ScriptEditor.ScriptEditor(scriptType, fileName);
            loadFile.OnSave += SaveScript;
            loadFile.RequireConfirmSaveDialog += OpenConfirmSaveDialog;
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Script", new object[] { loadFile, loadFile.ScriptObject });
            inspector.AddProperty(mapProperty);
            code.Children.Add(new CodeEditor(loadFile.ScriptObject, "Code"));
            gridSplitter.IsEnabled = true;
            codeColumn.Width = new GridLength(EditorPanel.ActualWidth / 2);
        }

        void OnChangeSelectedObject()
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
            switch (((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.GetSelectedObjectType())
            {
                case SatCore.MapEditor.SelectType.None:
                    break;
                case SatCore.MapEditor.SelectType.Box:
                    Property boxProperty = new Property("Box", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    inspector.AddProperty(boxProperty);
                    break;
                case SatCore.MapEditor.SelectType.Triangle:
                    Property triangleProperty = new Property("Triangle", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    inspector.AddProperty(triangleProperty);
                    break;
                case SatCore.MapEditor.SelectType.Door:
                    Property doorProperty = new Property("Door", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    inspector.AddProperty(doorProperty);
                    break;
                case SatCore.MapEditor.SelectType.Object:
                    Property mapObjectProperty = new Property("Map Object", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    inspector.AddProperty(mapObjectProperty);
                    break;
                case SatCore.MapEditor.SelectType.EventObject:
                    Property npcMapObjectProperty = new Property("Non-Player Character", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    inspector.AddProperty(npcMapObjectProperty);
                    break;
                case SatCore.MapEditor.SelectType.Event:
                    Property mapEventProperty = new Property("Event", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    inspector.AddProperty(mapEventProperty);
                    break;
                case SatCore.MapEditor.SelectType.CameraRestriction:
                    Property cameraRestrictionProperty = new Property("カメラ制限", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    inspector.AddProperty(cameraRestrictionProperty);
                    break;
                case SatCore.MapEditor.SelectType.SavePoint:
                    Property savePointProperty = new Property("セーブポイント", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    inspector.AddProperty(savePointProperty);
                    break;
                default:
                    break;
            }
        }

        void OnCreateDoor()
        {
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
            Property doorProperty = new Property("Door", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
            inspector.AddProperty(doorProperty);
        }

        void OnCreateMapObject()
        {
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
            Property mapObjectProperty = new Property("Map Object", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
            inspector.AddProperty(mapObjectProperty);
        }

        private void select_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Select;
            EditorPanel.Cursor = Cursors.Arrow;
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
        }

        private void box_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Box;
            EditorPanel.Cursor = Cursors.Cross;
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
        }

        private void triangle_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Triangle;
            EditorPanel.Cursor = Cursors.Cross;
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
        }

        private void door_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Door;
            EditorPanel.Cursor = Cursors.Arrow;
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
        }

        private void mapObject_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Object;
            EditorPanel.Cursor = Cursors.Arrow;
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
        }

        private void npc_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.EventObject;
            EditorPanel.Cursor = Cursors.Arrow;
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
        }

        private void mapEvent_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Event;
            EditorPanel.Cursor = Cursors.Cross;
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
        }

        private void Camera_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.CameraRestriction;
            EditorPanel.Cursor = Cursors.Cross;
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
        }

        private void SavePoint_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.SavePoint;
            EditorPanel.Cursor = Cursors.Cross;
            Reset(obj => obj is AltseedInspector.Property property && property.Title != "General" && property.Title != "Map");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene != null) SaveMap();
            if (asd.Engine.CurrentScene as SatCore.CharacterImageEditor.CharacterImageEditor != null) SaveCharacterImage();
            if (asd.Engine.CurrentScene as SatCore.ScriptEditor.ScriptEditor != null) SaveScript();
        }

        void SaveMap()
        {
            if (((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Path == "" || ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Path == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "new.map";
                saveFileDialog.InitialDirectory = "";
                saveFileDialog.Filter = "Map File|*.map";
                saveFileDialog.Title = "保存";
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() != true) return;
                ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Path = saveFileDialog.FileName;
            }
            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).SaveImp(((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Path);
        }

        void SaveCharacterImage()
        {
            if (((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).Path == "" || ((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).Path == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "new.ci";
                saveFileDialog.InitialDirectory = "";
                saveFileDialog.Filter = "Character Image File|*.ci";
                saveFileDialog.Title = "保存";
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() != true) return;
                ((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).Path = saveFileDialog.FileName;
            }
            ((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).SaveImp(((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).Path);
        }

        void SaveScript()
        {
            if (((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Path == "" || ((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Path == null)
            {
                SaveFileDialog saveFileDialog = GetSaveFileDialog(((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Script);
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() != true) return;
                ((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Path = saveFileDialog.FileName;
            }
            ((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).SaveImp(((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Path);
        }

        SaveFileDialog GetSaveFileDialog(SatCore.ScriptEditor.ScriptEditor.ScriptType scriptType)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = "";
            dialog.Title = "保存";
            dialog.RestoreDirectory = true;
            switch (scriptType)
            {
                case SatCore.ScriptEditor.ScriptEditor.ScriptType.MapObject:
                    dialog.FileName = "new.mobj";
                    dialog.Filter = "Map Object Script|*.mobj";
                    break;
                case SatCore.ScriptEditor.ScriptEditor.ScriptType.EventObject:
                    dialog.FileName = "new.eobj";
                    dialog.Filter = "Event Object Script|*.eobj";
                    break;
                case SatCore.ScriptEditor.ScriptEditor.ScriptType.Player:
                    dialog.FileName = "new.pc";
                    dialog.Filter = "Player Script|*.pc";
                    break;
                case SatCore.ScriptEditor.ScriptEditor.ScriptType.BackGround:
                    dialog.FileName = "new.bg";
                    dialog.Filter = "Back Ground Script|*.bg";
                    break;
                default:
                    Logger.Error(new NotImplementedException("実装されていません:" + scriptType.ToString()));
                    break;
            }
            return dialog;
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene != null) SaveAsMap();
            if (asd.Engine.CurrentScene as SatCore.CharacterImageEditor.CharacterImageEditor != null) SaveAsCharacterImage();
            if (asd.Engine.CurrentScene as SatCore.ScriptEditor.ScriptEditor != null) SaveAsScript();
        }

        void SaveAsMap()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "new.map";
            saveFileDialog.InitialDirectory = "";
            saveFileDialog.Filter = "Map File|*.map";
            saveFileDialog.Title = "別名で保存";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() != true) return;
            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Path = saveFileDialog.FileName;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).SaveImp(((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Path);
        }

        void SaveAsCharacterImage()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "new.ci";
            saveFileDialog.InitialDirectory = "";
            saveFileDialog.Filter = "Character Image File|*.ci";
            saveFileDialog.Title = "保存";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() != true) return;
            ((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).Path = saveFileDialog.FileName;

            ((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).SaveImp(((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).Path);
        }

        void SaveAsScript()
        {
            SaveFileDialog saveFileDialog = GetSaveFileDialog(((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Script);
            if (saveFileDialog.ShowDialog() != true) return;
            ((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Path = saveFileDialog.FileName;
            ((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).SaveImp(((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Path);
        }

        private void EditorPanel_Click(object sender, EventArgs e)
        {
            EditorPanel.Focus();
        }

        private void EditorPanel_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        string OpenCharacterImageFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "";
            openFileDialog.InitialDirectory = "";
            openFileDialog.Filter = "Character Image File|*.ci|All File|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != true) return "";

            return SatCore.Path.GetRelativePath(openFileDialog.FileName, RootPath);
        }

        private void undo_Click(object sender, RoutedEventArgs e)
        {
            SatCore.UndoRedoManager.Undo();
        }

        private void redo_Click(object sender, RoutedEventArgs e)
        {
            SatCore.UndoRedoManager.Redo();
        }

        void OnUpdateUndoRedoData()
        {
            undo.IsEnabled = SatCore.UndoRedoManager.IsCanUndo;
            redo.IsEnabled = SatCore.UndoRedoManager.IsCanRedo;
        }

        void Reset(Func<UIElement, bool> func)
        {
            code.Children.OfType<CodeEditor>().FirstOrDefault()?.Dispose();
            code.Children.Clear();
            gridSplitter.IsEnabled = false;
            codeColumn.Width = new GridLength(0);
            inspector.RemoveProperty(func);
        }

        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrames), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrames(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        private void Undo(object sender, ExecutedRoutedEventArgs e)
        {
            SatCore.UndoRedoManager.Undo();
        }

        private void Redo(object sender, ExecutedRoutedEventArgs e)
        {
            SatCore.UndoRedoManager.Redo();
        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene != null) SaveMap();
            if (asd.Engine.CurrentScene as SatCore.CharacterImageEditor.CharacterImageEditor != null) SaveCharacterImage();
            if (asd.Engine.CurrentScene as SatCore.ScriptEditor.ScriptEditor != null) SaveScript();
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = undo.IsEnabled;
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = redo.IsEnabled;
        }

        private void copy_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;
            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Copy();
        }

        private void paste_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;
            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Paste();
        }

        private void Copy(object sender, ExecutedRoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;
            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Copy();
        }

        private void Paste(object sender, ExecutedRoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;
            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Paste();
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = copy.IsEnabled;
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = paste.IsEnabled;
        }

        private void OnCopyObjectChanged(bool isCanCopy, bool isCanPaste)
        {
            copy.IsEnabled = isCanCopy;
            paste.IsEnabled = isCanPaste;
        }

        SatCore.BaseEditorScene.ConfirmSaveDialogResult OpenConfirmSaveDialog()
        {
            var result = System.Windows.MessageBox.Show("保存しますか？", "保存しますか？", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);

            switch (result)
            {
                case MessageBoxResult.Cancel:
                    return SatCore.BaseEditorScene.ConfirmSaveDialogResult.Cancel;
                case MessageBoxResult.Yes:
                    return SatCore.BaseEditorScene.ConfirmSaveDialogResult.Save;
                case MessageBoxResult.No:
                    return SatCore.BaseEditorScene.ConfirmSaveDialogResult.NotSave;
                default:
                    return SatCore.BaseEditorScene.ConfirmSaveDialogResult.NotSave;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (asd.Engine.CurrentScene is SatCore.BaseEditorScene scene)
                if (scene.ConfirmSave() == SatCore.BaseEditorScene.ConfirmSaveDialogResult.Cancel)
                    e.Cancel = true;
                else
                    scene.RemoveEvent();
        }
    }
}
