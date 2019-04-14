using BaseComponent;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
using SatCore.Attribute;
using SatPlayer;
using SatPlayer.Game;
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
    /// <summary>
    /// 編集可能マップオブジェクト
    /// </summary>
    public class EditableMapObject : MapObject, INotifyPropertyChanged, IScriptObject
    {
        private string _code;
        private bool isEdited;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool IsSuccessBuild { get; set; }

        public EditableMapObject()
        {
        }

        [Script("スクリプト", "MapObject")]
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

        public bool IsSingle => false;

        public bool IsPreparePlayer => true;

        public string ScriptOptionName => "MapObject";

        [Button("Run")]
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
            sensors = new Dictionary<string, Sensor>();
            Effects = new Dictionary<string, Effect>();
            childMapObjectData = new Dictionary<string, MapObject>();
            Update = (obj) => { };
        }
    }
}
