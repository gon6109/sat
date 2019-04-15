using BaseComponent;
using Microsoft.CodeAnalysis.Scripting;
using SatCore.Attribute;
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

        public EditableBackGround() : base()
        {

        }

        public bool IsSuccessBuild { get; private set; }

        public bool IsSingle => false;

        public bool IsPreparePlayer => true;

        public string ScriptOptionName => "BackGround";

        /// <summary>
        /// OnUpdade時に呼び出されるイベント
        /// </summary>
        public override event Action<SatScript.BackGround.IBackGround> Update = delegate { };

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

        public new object Clone()
        {
            var clone = new EditableBackGround();
            clone.Copy(this);
            clone.State = State;
            clone.Zoom = Zoom;
            clone.Update = Update;
            clone.UpdatePriority = UpdatePriority;
            return clone;
        }

        void Reset()
        {
            AnimationPart.Clear();
            Update = delegate { };
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            Camera.IsDrawn = false;
        }

        protected override void OnUpdate()
        {
            Update(this);
            base.OnUpdate();
        }

        protected override void OnDispose()
        {
            Camera.Dispose();
            base.OnDispose();
        }
    }
}
