﻿using SatIO;
using PhysicAltseed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BaseComponent;
using SatPlayer;
using Microsoft.CodeAnalysis.Scripting;
using SatCore.Attribute;
using System.IO;

namespace SatCore.MapEditor
{
    /// <summary>
    /// Event対応キャラクター
    /// </summary>
    public class EventObject : SatPlayer.Game.Object.EventObject, ICopyPasteObject, IMovable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _scriptPath;

        [TextOutput("ID")]
        public new int ID { get => base.ID; set => base.ID = value; }

        [VectorInput("座標")]
        public new asd.Vector2DF Position { get => base.Position; set => base.Position = value; }

        /// <summary>
        /// スクリプトへのパス
        /// </summary>
        [FileInput("スクリプト", "EventObject File|*.eobj|All File|*.*")]
        public string ScriptPath
        {
            get => _scriptPath;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _scriptPath = value;
                Reset();
                Script<object> script;
                try
                {
                    using (var stream = IO.GetStream(_scriptPath))
                        script = ScriptOption.ScriptOptions["EventObject"]?.CreateScript<object>(Encoding.UTF8.GetString(stream.ToArray()));
                    script.Compile();
                    var task = script.RunAsync(this);
                    task.Wait();
                    State = AnimationPart.FirstOrDefault().Key;
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
                if (Texture == null)
                    Texture = TextureManager.LoadTexture("");
                CenterPosition = Texture.Size.To2DF() / 2;
                if (CollisionShape is PhysicalRectangleShape shape)
                    shape.DrawingArea = new asd.RectF(Position - CenterPosition, Texture.Size.To2DF());
                OnPropertyChanged();
            }
        }

        public EventObject()
        {
            IsUpdated = false;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            if (Texture == null)
                Texture = TextureManager.LoadTexture("");
            if (Layer is MapLayer map)
            {
                collision = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, map.PhysicalWorld);
                if (CollisionShape is PhysicalRectangleShape shape)
                    shape.DrawingArea = new asd.RectF(Position - CenterPosition, Texture.Size.To2DF());
            }
        }

        [Button("消去")]
        public void OnClickRemove()
        {
            UndoRedoManager.ChangeObject2D(Layer, this, false);
            Layer.RemoveObject(this);
        }

        public ICopyPasteObject Copy()
        {
            UndoRedoManager.Enable = false;
            EventObject copy = new EventObject();
            copy.ScriptPath = ScriptPath;
            copy.Position = Position + new asd.Vector2DF(50, 50);
            return copy;
        }

        asd.Vector2DF pos;

        public void StartMove()
        {
            pos = Position;
        }

        public void EndMove()
        {
            UndoRedoManager.ChangeProperty(this, Position, pos, "Position");
        }

        public EventObjectIO ToIO()
        {
            var result = new EventObjectIO()
            {
                ScriptPath = ScriptPath,
                Position = Position,
                ID = ID,
            };
            return result;
        }

        public static EventObject CreateEventObject(EventObjectIO mapObject)
        {
            var eventObject = new EventObject();
            eventObject.ScriptPath = mapObject.ScriptPath;
            eventObject.Position = mapObject.Position;
            eventObject.ID = mapObject.ID;
            return eventObject;
        }
    }
}
