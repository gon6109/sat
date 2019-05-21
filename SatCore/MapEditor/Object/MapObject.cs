using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BaseComponent;
using SatIO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
using SatCore.Attribute;

namespace SatCore.MapEditor.Object
{
    /// <summary>
    /// マップオブジェクト
    /// </summary>
    public class MapObject : MultiAnimationObject2D, INotifyPropertyChanged, IMovable, ICopyPasteObject, IMapElement
    {
        static ScriptOptions options = ScriptOptions.Default.WithImports("SatPlayer", "PhysicAltseed", "System")
                                     .WithReferences(System.Reflection.Assembly.GetAssembly(typeof(MapObject))
                                                     , System.Reflection.Assembly.GetAssembly(typeof(asd.Vector2DF))
                                                     , System.Reflection.Assembly.GetAssembly(typeof(PhysicalRectangleShape)));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [VectorInput("座標")]
        public new asd.Vector2DF Position
        {
            get
            {
                return base.Position;
            }

            set
            {
                base.Position = value;
                CollisionShape.DrawingArea = new asd.RectF(value - Texture.Size.To2DF() / 2.0f, CollisionShape.DrawingArea.Size);
                OnPropertyChanged();
            }
        }

        private string _scriptPath;

        /// <summary>
        /// スクリプトへのパス
        /// </summary>
        [FileInput("スクリプト", "MapObject File|*.obj|All File|*.*")]
        public string ScriptPath
        {
            get => _scriptPath;
            set
            {
                if (value != null && asd.Engine.File.Exists(value))
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    _scriptPath = value;
                    OnPropertyChanged();
                    AnimationPart.Clear();
                    try
                    {
                        StreamReader reader = new StreamReader(IO.GetStream(_scriptPath));
                        string code = "";
                        string temp;
                        while ((temp = reader.ReadLine()) != null)
                        {
                            if (temp.IndexOf("AddAnimationPart(") > -1) code += temp + "\n";
                        }
                        Script<object> script = CSharpScript.Create(code, options: options, globalsType: typeof(MapObject));
                        var thread = script.RunAsync(this);
                        thread.Wait();
                        State = AnimationPart.First().Key;
                    }
                    catch (Exception e)
                    {
                        ErrorIO.AddError(e);
                        Texture = TextureManager.LoadTexture("Static/error.png");
                    }
                }
                else Texture = TextureManager.LoadTexture("Static/error.png");
                CenterPosition = Texture.Size.To2DF() / 2;
                CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition, Texture.Size.To2DF());
            }
        }

        public asd.RectangleShape CollisionShape { get; }

        public asd.Vector2DF BottomRight => Position + CenterPosition;

        public MapObject()
        {
            CollisionShape = new asd.RectangleShape();
            Color = new asd.Color(255, 255, 255, 200);
            DrawingPriority = 2;
            Texture = TextureManager.LoadTexture("Static/error.png");
            ScriptPath = "";
        }

        [Button("消去")]
        public void OnClickRemove()
        {
            UndoRedoManager.ChangeObject2D(Layer, this, false);
            Layer.RemoveObject(this);
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

        public ICopyPasteObject Copy()
        {
            MapObject copy = new MapObject();
            copy.ScriptPath = ScriptPath;
            copy.Position = Position + new asd.Vector2DF(50, 50);
            return copy;
        }

        public MapObjectIO ToIO()
        {
            var result = new MapObjectIO()
            {
                ScriptPath = ScriptPath,
                Position = Position,
            };
            return result;
        }

        public static MapObject CreateMapObject(MapObjectIO mapObjectIO)
        {
            var mapObject = new MapObject();
            mapObject.Color = new asd.Color(255, 255, 255, 200);
            mapObject.DrawingPriority = 2;
            mapObject.ScriptPath = mapObjectIO.ScriptPath;
            mapObject.Position = mapObjectIO.Position;
            return mapObject;
        }
    }
}
