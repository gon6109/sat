using BaseComponent;
using Microsoft.CodeAnalysis.Scripting;
using SatPlayer;
using SatPlayer.Game.Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.ScriptEditor
{
    public class EditableBackGround : BackGround, IScriptObject, ICloneable
    {
        private bool isEdited;
        private string _code;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [Script("スクリプト", "EventObject")]
        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                isEdited = true;
                OnPropertyChanged();
            }
        }

        public EditableBackGround(MainMapLayer2D layer) : base(layer)
        {

        }

        public bool IsSuccessBuild { get; private set; }

        public bool IsSingle => false;

        public bool IsPreparePlayer => true;

        public string ScriptOptionName => "BackGround";

        [Button("ビルド")]
        public void Run()
        {
            if (isEdited)
            {
                IsSuccessBuild = true;
                try
                {
                    Reset();
                    Script<object> script = ScriptOption.ScriptOptions[ScriptOptionName]?.CreateScript<object>(Code);
                    var thread = script.RunAsync(this);
                    thread.Wait();
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                    IsSuccessBuild = false;
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            isEdited = false;
        }

        void Reset()
        {
            AnimationPart.Clear();
            Update = (obj) => { };
        }

        public new  object Clone()
        {
            EditableBackGround clone = new EditableBackGround(Layer as MainMapLayer2D);
            clone.Clone(this);
            clone.State = State;
            clone.Zoom = Zoom;
            clone.Update = Update;
            clone.UpdatePriority = UpdatePriority;
            return clone;
        }

        protected override void OnAdded()
        {
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        protected override void OnDispose()
        {
            Camera.Dispose();
            base.OnDispose();
        }
    }
}
