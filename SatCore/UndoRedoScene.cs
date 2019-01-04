using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    public class UndoRedoScene : asd.Scene
    {
        public UndoRedoScene()
        {
            ErrorIO.Scene = this;
        }

        protected override void OnStartUpdating()
        {
            UndoRedoManager.Reset();
            base.OnStopUpdating();
        }
    }
}
