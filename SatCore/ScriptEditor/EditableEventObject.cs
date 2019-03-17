﻿using BaseComponent;
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

namespace SatCore.ScriptEditor
{
    public class EditableEventObject : SatPlayer.EventObject, IScriptObject
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
                    var eventObjects = asd.Engine.CurrentScene.Layers.OfType<MainMapLayer2D>()?
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

        public EditableEventObject(PhysicalWorld world)
        {
            refWorld = world;
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
            EditableEventObject clone = new EditableEventObject(refWorld);
            clone.sensors = new Dictionary<string, Sensor>(sensors);
            clone.childMapObjectData = new Dictionary<string, MapObject>(childMapObjectData);
            clone.Effects = new Dictionary<string, Effect>(Effects);
            clone.refWorld = refWorld;
            clone.Update = Update;
            clone.State = State;
            clone.Tag = Tag;
            clone.Clone(this);
            clone.MapObjectType = MapObjectType;
            try
            {
                clone.collisionShape.DrawingArea = new asd.RectF(new asd.Vector2DF(), clone.AnimationPart.First().Value.Textures.First().Size.To2DF());
            }
            catch (Exception e)
            {
                ErrorIO.AddError(e);
            }
            clone.CenterPosition = clone.collisionShape.DrawingArea.Size / 2;
            if (MapObjectType == SatScript.MapObject.MapObjectType.Active)
            {
                clone.CollisionShape.GroupIndex = CollisionShape.GroupIndex;
                clone.CollisionShape.MaskBits = CollisionShape.MaskBits;
                clone.CollisionShape.CategoryBits = CollisionShape.CategoryBits;
            }
            clone.IsEvent = IsEvent;
            return clone;
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