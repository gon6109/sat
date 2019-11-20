using BaseComponent;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
using InspectorModel;
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

        public EditablePlayer()
        {
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

        /// <summary>
        /// OnUpdate時に呼び出されイベント
        /// </summary>
        public override event Action<SatScript.Player.IPlayer> Update = delegate { };

        protected override void OnAdded()
        {
            base.OnAdded();
            if (Layer is MapLayer map && CollisionShape == null)
                CollisionShape = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, map.PhysicalWorld);
            SetCollision();
        }

        protected override void OnUpdate()
        {
            try
            {
                Update(this);
            }
            catch (Exception e)
            {
                Update = delegate { };
                Logger.Error(e);
            }
            base.OnUpdate();
        }

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
                    if (CollisionShape != null)
                    {
                        CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition + new asd.Vector2DF(5, 0), Texture.Size.To2DF() - new asd.Vector2DF(10, 0));
                        GroundCollision.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);
                    }
                    Position = ScalingLayer2D.OriginDisplaySize / 2;
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

        void Reset()
        {
            Update = delegate { };
            AnimationPart.Clear();
            Effects.Clear();
        }
    }
}
