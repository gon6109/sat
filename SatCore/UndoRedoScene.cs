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
            ErrorIO.Scene = this;
            asd.Engine.Reload();
        }

        protected override void OnStartUpdating()
        {
            UndoRedoManager.Reset();
            base.OnStopUpdating();
        }
    }
}
