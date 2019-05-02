using SatIO;
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
using SatScript.MapObject;

namespace SatCore.MapEditor
{
    /// <summary>
    /// Event対応キャラクター
    /// </summary>
    public class EventObject : SatPlayer.Game.Object.EventObject, ICopyPasteObject, IMovable, IActor
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _scriptPath;
        private asd.Vector2DF _position;
        bool _isActivePhysic;

        [TextOutput("ID")]
        public new int ID { get => base.ID; set => base.ID = value; }

        [VectorInput("座標")]
        public asd.Vector2DF StartPosition
        {
            get => _position;
            set
            {
                base.Position = value;
                _position = value;
                OnPropertyChanged();
            }
        }

        public new asd.Vector2DF Position
        {
            get => base.Position;
            set => base.Position = value;
        }

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

        PhysicalShape IActor.CollisionShape => CollisionShape as PhysicalShape;

        public bool IsActivePhysic
        {
            get => _isActivePhysic;
            set
            {
                _isActivePhysic = value;
                if (CollisionShape is PhysicalShape shape)
                    shape.IsActive = value;
            }
        }

        public EventObject()
        {
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            if (Texture == null)
                Texture = TextureManager.LoadTexture("");
            SetCollision();
        }

        protected override void OnDispose()
        {
            if (collision is PhysicalShape shape)
                shape.Dispose();
            collision = null;
        }

        protected override void OnRemoved()
        {
            if (collision is PhysicalShape shape)
                shape.Dispose();
            collision = null;
        }

        protected override void OnUpdate()
        {
            if (!IsEvent)
            {
                UpdatePhysic();
                if (!AnimationPart.ContainsKey(State) || !IsAnimate) return;
                if (AnimationPart[State].Update() && IsOneLoop)
                {
                    State = PreState;
                    IsOneLoop = false;
                }
                if (AnimationPart[State].IsUpdated)
                {
                    Texture = AnimationPart[State].CurrentTexture;
                    AnimationPart[State].IsUpdated = false;
                }
            }
            else
                base.OnUpdate();
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
            copy.Position = StartPosition + new asd.Vector2DF(50, 50);
            return copy;
        }

        asd.Vector2DF pos;

        public void StartMove()
        {
            pos = StartPosition;
        }

        public void EndMove()
        {
            UndoRedoManager.ChangeProperty(this, StartPosition, pos, "StartPosition");
        }

        public EventObjectIO ToIO()
        {
            var result = new EventObjectIO()
            {
                ScriptPath = ScriptPath,
                Position = StartPosition,
                ID = ID,
            };
            return result;
        }

        public static EventObject CreateEventObject(EventObjectIO mapObject)
        {
            var eventObject = new EventObject();
            eventObject.ScriptPath = mapObject.ScriptPath;
            eventObject.StartPosition = mapObject.Position;
            eventObject.ID = mapObject.ID;
            return eventObject;
        }

        public void SetCollision(MapLayer mapLayer)
        {
            if (collision is PhysicalShape shape)
                shape.Dispose();
            collision = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, mapLayer.PhysicalWorld);
            if (CollisionShape is PhysicalRectangleShape shape2)
            {
                shape2.DrawingArea = new asd.RectF(Position - CenterPosition, Texture?.Size.To2DF() ?? default);
                shape2.IsActive = IsActivePhysic;
            }
        }

        void SetCollision()
        {
            if (CollisionShape == null)
                collision = new asd.RectangleShape();

            if (CollisionShape is asd.RectangleShape shape2)
            {
                shape2.DrawingArea = new asd.RectF(Position - CenterPosition, Texture?.Size.To2DF() ?? default);
            }
        }

        void IActor.OnUpdate()
        {
            OnUpdate();
        }
    }
}
