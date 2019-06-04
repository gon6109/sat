using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    /// <summary>
    /// エディターシーン
    /// </summary>
    public class BaseEditorScene : asd.Scene
    {
        public enum ConfirmSaveDialogResult
        {
            Save,
            NotSave,
            Cancel
        }

        public string Path { get; set; }

        bool IsEdit { get; set; }

        public event Func<ConfirmSaveDialogResult> RequireConfirmSaveDialog = delegate { return ConfirmSaveDialogResult.NotSave; };

        public event Action OnSave = delegate { };

        public BaseEditorScene()
        {
            var printer = new LogPrintLayer2D();
            printer.DrawingPriority = 3;
            Logger.Printer = printer;
            AddLayer(printer);
            asd.Engine.Reload();
        }

        protected override void OnStartUpdating()
        {
            UndoRedoManager.Reset();
            UndoRedoManager.OnUpdateData += OnUpdateData;
            base.OnStopUpdating();
        }

        private void OnUpdateData()
        {
            IsEdit = true;
        }

        protected override void OnUnregistered()
        {
            base.OnUnregistered();
            RequireConfirmSaveDialog = delegate { return ConfirmSaveDialogResult.NotSave; };
        }

        public ConfirmSaveDialogResult ConfirmSave()
        {
            ConfirmSaveDialogResult result = ConfirmSaveDialogResult.NotSave;
            if (IsEdit &&
                (UndoRedoManager.IsCanUndo || UndoRedoManager.IsCanUndo))
                result = RequireConfirmSaveDialog();
            if (result == ConfirmSaveDialogResult.Save)
                Save();
            return result;
        }

        public virtual void SaveImp(string path)
        {
            IsEdit = false;
        }

        public void Save()
        {
            IsEdit = false;
            OnSave();
        }

        public void RemoveEvent()
        {
            UndoRedoManager.OnUpdateData -= OnUpdateData;
            OnSave = delegate { };
        }
    }
}
