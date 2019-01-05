using BaseComponent;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
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
        static ScriptOptions options = ScriptOptions.Default.WithImports("SatPlayer", "PhysicAltseed", "System", "System.Collections.Generic")
                                                         .WithReferences(System.Reflection.Assembly.GetAssembly(typeof(IEnumerator<>))
                                                                         , System.Reflection.Assembly.GetAssembly(typeof(SatPlayer.MapObject))
                                                                         , System.Reflection.Assembly.GetAssembly(typeof(asd.Vector2DF))
                                                                         , System.Reflection.Assembly.GetAssembly(typeof(PhysicalRectangleShape)));
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
                    Script<object> script = CSharpScript.Create(Code, options: options, globalsType: typeof(SatPlayer.MapObject));
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
            sounds = new Dictionary<string, BaseComponent.Sound>();
            Update = (obj) => { };
        }
    }
}
