using BaseComponent;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
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
    public class EditablePlayer : Player, IScriptObject
    {
        private bool isEdited;
        private string _code;

        public EditablePlayer(PhysicalWorld physicalWorld)
        {
            CollisionShape = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, physicalWorld);
        }

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

        public bool IsSingle => true;

        public bool IsPreparePlayer => false;

        public string ScriptOptionName => "Player";

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
                    CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition + new asd.Vector2DF(5, 0), Texture.Size.To2DF() - new asd.Vector2DF(10, 0));
                    GroundCollision.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);
                    Position = ScalingLayer2D.OriginDisplaySize / 2; 
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
            Init();
        }
    }
}
