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
        public async Task Run()
        {
            if (isEdited)
            {
                IsSuccessBuild = true;
                try
                {
                    Reset();
                    Script<object> script = ScriptOption.ScriptOptions[ScriptOptionName]?.CreateScript<object>(Code);
                    await script.RunAsync(this);
                    foreach (var item in LoadTextureTasks)
                    {
                        AddAnimationPart(item.animationGroup, item.extension, item.sheets, item.partName, item.interval);
                    }
                    State = State;
                    LoadTextureTasks.Clear();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    IsSuccessBuild = false;
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            isEdited = false;
        }

        public new object Clone()
        {
            var clone = new EditableMapObject();
            CloneImp(clone, true);
            return clone;
        }
    }
}
