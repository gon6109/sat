using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    /// <summary>
    /// UndoRedo実装シーン
    /// </summary>
    public class UndoRedoScene : asd.Scene
    {
        public UndoRedoScene()
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
            base.OnStopUpdating();
        }
    }
}
