using BaseComponent;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
using SatPlayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapObjectEditor
{
    /// <summary>
    /// 編集可能マップオブジェクト
    /// </summary>
    public class EditableMapObject : SatPlayer.MapObject, INotifyPropertyChanged
    {
        private string _code;
        private bool isEdited;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool IsSuccessBuild { get; set; }

        public EditableMapObject(PhysicalWorld world)
        {
            refWorld = world;
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

        [Button("Run")]
        public void Run()
        {
            if (isEdited) 
            {
                IsSuccessBuild = true;
                try
                {
                    Reset();
                    Script<object> script = ScriptOption.ScriptOptions["MapObject"]?.CreateScript<object>(Code);
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
            Effects = new Dictionary<string, SatPlayer.Effect>();
            childMapObjectData = new Dictionary<string, SatPlayer.MapObject>();
            Update = (obj) => { };
        }
    }
}
