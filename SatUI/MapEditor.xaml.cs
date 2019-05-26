using BaseComponent;
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

        string playerExePath = "";
        [SatCore.Attribute.DirectoryInput("本体へのパス", false)]
        public string PlayerExePath
        {
            get => playerExePath;
            set
            {
                if (asd.Engine.File == null) return;

                asd.Engine.File.ClearRootDirectories();
                try
                {
                    asd.Engine.File.AddRootDirectory(System.IO.Path.GetDirectoryName(value));
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

                playerExePath = value;
                PlayerExePathGetter = playerExePath;

                SatCore.PlayersListDialog.CheckPlayersList(value);
                OnPropertyChanged();
            }
        }
        public static string PlayerExePathGetter { get; private set; }

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

            propertyPanel.AddProperty(mapProperty);
            SatCore.UndoRedoManager.OnUpdateData += OnUpdateUndoRedoData;
            gridSplitter.IsEnabled = false;
            codeColumn.Width = new GridLength(0);
        }

        private void map_Click(object sender, RoutedEventArgs e)
        {
            var newFile = new SatCore.MapEditor.MapEditorScene();
            newFile.OnRequestShowProgressDialog += ShowProgressDialog;
            newFile.Map.OnChangeSelectedObject += OnChangeSelectedObject;
            newFile.Map.OnCreateDoor += OnCreateDoor;
            newFile.Map.OnCreateMapObject += OnCreateMapObject;
            newFile.Map.FocusToEditorPanel += () => EditorPanel.Focus();
            newFile.Map.RequireOpenFileDialog += OpenCharacterImageFileDialog;
            newFile.OnCopyObjectChanged += OnCopyObjectChanged;
            asd.Engine.ChangeScene(newFile);
            Reset(PropertyPanel.ResetMode.General);

            Property mapProperty = new Property("Map", new object[] { newFile.Map, newFile });
            propertyPanel.AddProperty(mapProperty);
        }

        private void ShowProgressDialog(string title, string bindingPath, INotifyPropertyChanged bindingSource)
        {
            var dialog = new ProgressDialog(title, bindingPath, bindingSource);
            dialog.Owner = GetWindow(this);
            dialog.Show();
        }

        private void characterImage_Click(object sender, RoutedEventArgs e)
        {
            Reset(PropertyPanel.ResetMode.General);

            var loadFile = new SatCore.CharacterImageEditor.CharacterImageEditor();
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Chracter Image", loadFile.Character);
            propertyPanel.AddProperty(mapProperty);
        }

        private void CreateMapObject_Click(object sender, RoutedEventArgs e)
        {
            Reset(PropertyPanel.ResetMode.General);

            var loadFile = new SatCore.ScriptEditor.ScriptEditor(SatCore.ScriptEditor.ScriptEditor.ScriptType.MapObject);
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Map Object", new object[] { loadFile, loadFile.ScriptObject });
            propertyPanel.AddProperty(mapProperty);
            code.Children.Add(new CodeEditor(loadFile.ScriptObject, "Code"));
            gridSplitter.IsEnabled = true;
            codeColumn.Width = new GridLength(EditorPanel.ActualWidth / 2);
        }

        private void CreateEventObject_Click(object sender, RoutedEventArgs e)
        {
            Reset(PropertyPanel.ResetMode.General);

            var loadFile = new SatCore.ScriptEditor.ScriptEditor(SatCore.ScriptEditor.ScriptEditor.ScriptType.EventObject);
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Map Object", new object[] { loadFile, loadFile.ScriptObject });
            propertyPanel.AddProperty(mapProperty);
            code.Children.Add(new CodeEditor(loadFile.ScriptObject, "Code"));
            gridSplitter.IsEnabled = true;
            codeColumn.Width = new GridLength(EditorPanel.ActualWidth / 2);
        }

        private void CreatePlayer_Click(object sender, RoutedEventArgs e)
        {
            Reset(PropertyPanel.ResetMode.General);

            var loadFile = new SatCore.ScriptEditor.ScriptEditor(SatCore.ScriptEditor.ScriptEditor.ScriptType.Player);
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Map Object", new object[] { loadFile, loadFile.ScriptObject });
            propertyPanel.AddProperty(mapProperty);
            code.Children.Add(new CodeEditor(loadFile.ScriptObject, "Code"));
            gridSplitter.IsEnabled = true;
            codeColumn.Width = new GridLength(EditorPanel.ActualWidth / 2);
        }

        private void CreateBackGround_Click(object sender, RoutedEventArgs e)
        {
            Reset(PropertyPanel.ResetMode.General);

            var loadFile = new SatCore.ScriptEditor.ScriptEditor(SatCore.ScriptEditor.ScriptEditor.ScriptType.BackGround);
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Map Object", new object[] { loadFile, loadFile.ScriptObject });
            propertyPanel.AddProperty(mapProperty);
            code.Children.Add(new CodeEditor(loadFile.ScriptObject, "Code"));
            gridSplitter.IsEnabled = true;
            codeColumn.Width = new GridLength(EditorPanel.ActualWidth / 2);
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "";
            openFileDialog.InitialDirectory = "";
            openFileDialog.Filter = "All Readable File|*.map;*.pc;*.ci;*.bg;*.obj;*.eobj" +
                "|Map File|*.map|Player Script|*.pc|Character Image File|*.ci|Map Object Script|*.obj|Event Object Script|*.eobj|Back Ground Script|*.bg|All File|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != true) return;

            Reset(PropertyPanel.ResetMode.General);

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
                await loadFile.LoadMapData(fileName);
                asd.Engine.ChangeScene(loadFile);
                Property mapProperty = new Property("Map", new object[] { loadFile.Map, loadFile });
                propertyPanel.AddProperty(mapProperty);
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
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Chracter Image", loadFile.Character);
            propertyPanel.AddProperty(mapProperty);
        }

        void OpenScriptFile(string fileName)
        {
            SatCore.ScriptEditor.ScriptEditor.ScriptType scriptType = new SatCore.ScriptEditor.ScriptEditor.ScriptType();

            if (fileName.Contains(".obj")) scriptType = SatCore.ScriptEditor.ScriptEditor.ScriptType.MapObject;
            else if (fileName.Contains(".eobj")) scriptType = SatCore.ScriptEditor.ScriptEditor.ScriptType.EventObject;
            else if (fileName.Contains(".pc")) scriptType = SatCore.ScriptEditor.ScriptEditor.ScriptType.Player;
            else if (fileName.Contains(".bg")) scriptType = SatCore.ScriptEditor.ScriptEditor.ScriptType.BackGround;
            else
            {
                Logger.Error(new NotImplementedException("対応していない拡張子です。"));
                return;
            }

            var loadFile = new SatCore.ScriptEditor.ScriptEditor(scriptType, fileName);
            asd.Engine.ChangeScene(loadFile);
            Property mapProperty = new Property("Script", new object[] { loadFile, loadFile.ScriptObject });
            propertyPanel.AddProperty(mapProperty);
            code.Children.Add(new CodeEditor(loadFile.ScriptObject, "Code"));
            gridSplitter.IsEnabled = true;
            codeColumn.Width = new GridLength(EditorPanel.ActualWidth / 2);
        }

        void OnChangeSelectedObject()
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            Reset(PropertyPanel.ResetMode.Map);
            switch (((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.GetSelectedObjectType())
            {
                case SatCore.MapEditor.SelectType.None:
                    break;
                case SatCore.MapEditor.SelectType.Box:
                    Property boxProperty = new Property("Box", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    propertyPanel.AddProperty(boxProperty);
                    break;
                case SatCore.MapEditor.SelectType.Triangle:
                    Property triangleProperty = new Property("Triangle", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    propertyPanel.AddProperty(triangleProperty);
                    break;
                case SatCore.MapEditor.SelectType.Door:
                    Property doorProperty = new Property("Door", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    propertyPanel.AddProperty(doorProperty);
                    break;
                case SatCore.MapEditor.SelectType.Object:
                    Property mapObjectProperty = new Property("Map Object", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    propertyPanel.AddProperty(mapObjectProperty);
                    break;
                case SatCore.MapEditor.SelectType.EventObject:
                    Property npcMapObjectProperty = new Property("Non-Player Character", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    propertyPanel.AddProperty(npcMapObjectProperty);
                    break;
                case SatCore.MapEditor.SelectType.Event:
                    Property mapEventProperty = new Property("Event", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    propertyPanel.AddProperty(mapEventProperty);
                    break;
                case SatCore.MapEditor.SelectType.CameraRestriction:
                    Property cameraRestrictionProperty = new Property("カメラ制限", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    propertyPanel.AddProperty(cameraRestrictionProperty);
                    break;
                case SatCore.MapEditor.SelectType.SavePoint:
                    Property savePointProperty = new Property("セーブポイント", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
                    propertyPanel.AddProperty(savePointProperty);
                    break;
                default:
                    break;
            }
        }

        void OnCreateDoor()
        {
            Reset(PropertyPanel.ResetMode.Map);
            Property doorProperty = new Property("Door", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
            propertyPanel.AddProperty(doorProperty);
        }

        void OnCreateMapObject()
        {
            Reset(PropertyPanel.ResetMode.Map);
            Property mapObjectProperty = new Property("Map Object", ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.SelectedObject);
            propertyPanel.AddProperty(mapObjectProperty);
        }

        private void select_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Select;
            EditorPanel.Cursor = Cursors.Arrow;
            Reset(PropertyPanel.ResetMode.Map);
        }

        private void box_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Box;
            EditorPanel.Cursor = Cursors.Cross;
            Reset(PropertyPanel.ResetMode.Map);
        }

        private void triangle_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Triangle;
            EditorPanel.Cursor = Cursors.Cross;
            Reset(PropertyPanel.ResetMode.Map);
        }

        private void door_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Door;
            EditorPanel.Cursor = Cursors.Arrow;
            Reset(PropertyPanel.ResetMode.Map);
        }

        private void mapObject_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Object;
            EditorPanel.Cursor = Cursors.Arrow;
            Reset(PropertyPanel.ResetMode.Map);
        }

        private void npc_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.EventObject;
            EditorPanel.Cursor = Cursors.Arrow;
            Reset(PropertyPanel.ResetMode.Map);
        }

        private void mapEvent_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.Event;
            EditorPanel.Cursor = Cursors.Cross;
            Reset(PropertyPanel.ResetMode.Map);
        }

        private void Camera_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.CameraRestriction;
            EditorPanel.Cursor = Cursors.Cross;
            Reset(PropertyPanel.ResetMode.Map);
        }

        private void SavePoint_Click(object sender, RoutedEventArgs e)
        {
            if (asd.Engine.CurrentScene as SatCore.MapEditor.MapEditorScene == null) return;

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Map.CurrentToolType = SatCore.MapEditor.ToolType.SavePoint;
            EditorPanel.Cursor = Cursors.Cross;
            Reset(PropertyPanel.ResetMode.Map);
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
            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).SaveMapData(((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Path);
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
            ((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).SaveCharacterImage(((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).Path);
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
            ((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).SaveScript(((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Path);
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
                    dialog.FileName = "new.obj";
                    dialog.Filter = "Map Object Script|*.obj";
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

            ((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).SaveMapData(((SatCore.MapEditor.MapEditorScene)asd.Engine.CurrentScene).Path);
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

            ((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).SaveCharacterImage(((SatCore.CharacterImageEditor.CharacterImageEditor)asd.Engine.CurrentScene).Path);
        }

        void SaveAsScript()
        {
            SaveFileDialog saveFileDialog = GetSaveFileDialog(((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Script);
            if (saveFileDialog.ShowDialog() != true) return;
            ((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Path = saveFileDialog.FileName;
            ((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).SaveScript(((SatCore.ScriptEditor.ScriptEditor)asd.Engine.CurrentScene).Path);
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

            return SatCore.Path.GetRelativePath(openFileDialog.FileName, PlayerExePathGetter);
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

        void Reset(PropertyPanel.ResetMode mode)
        {
            code.Children.OfType<CodeEditor>().FirstOrDefault()?.Dispose();
            code.Children.Clear();
            gridSplitter.IsEnabled = false;
            codeColumn.Width = new GridLength(0);
            propertyPanel.ResetProperty(mode);
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

    }
}
