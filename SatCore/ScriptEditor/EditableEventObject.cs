using BaseComponent;
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
    public class EditableEventObject : EventObject, IScriptObject
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

        public bool IsSuccessBuild { get; private set; }

        [Button("ビルド")]
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

        bool isChanging;
        [BoolInput("イベント状態")]
        public new bool IsEvent
        {
            get => base.IsEvent;
            set
            {
                base.IsEvent = value;
                if (!isChanging)
                {
                    isChanging = true;
                    var eventObjects = asd.Engine.CurrentScene.Layers.OfType<MapLayer>()?
                        .FirstOrDefault()?.Objects.OfType<EditableEventObject>();
                    foreach (var item in eventObjects)
                    {
                        item.IsEvent = value;
                    }
                    isChanging = false;
                }
            }
        }

        public bool IsSingle => false;

        public bool IsPreparePlayer => true;

        public string ScriptOptionName => "EventObject";

        public EditableEventObject()
        {
        }

        protected override void OnUpdate()
        {
            if (IsEvent)
            {
                var moveCommand = new Dictionary<Inputs, bool>();
                foreach (Inputs item in Enum.GetValues(typeof(Inputs)))
                {
                    moveCommand[item] = Input.GetInputState(item) > 0;
                }
                MoveCommands.Enqueue(moveCommand);
            }

            base.OnUpdate();
        }

        public new object Clone()
        {
            EditableEventObject clone = new EditableEventObject();
            CloneImp((EventObject)clone, true);
            return clone;
        }
    }
}
