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

namespace SatCore.MapEditor
{
    /// <summary>
    /// マップオブジェクト
    /// </summary>
    public class MapObject : MultiAnimationObject2D, INotifyPropertyChanged, IMovable, ICopyPasteObject
    {
        static ScriptOptions options = ScriptOptions.Default.WithImports("SatPlayer", "PhysicAltseed", "System")
                                     .WithReferences(System.Reflection.Assembly.GetAssembly(typeof(MapObject))
                                                     , System.Reflection.Assembly.GetAssembly(typeof(asd.Vector2DF))
                                                     , System.Reflection.Assembly.GetAssembly(typeof(PhysicalRectangleShape)));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
                        while (( temp = reader.ReadLine()) != null)
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
                        Texture = TextureManager.LoadTexture("walk_g0000.png");
                    }
                }
                else Texture = TextureManager.LoadTexture("walk_g0000.png");
                CenterPosition = Texture.Size.To2DF() / 2;
                CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition, Texture.Size.To2DF());
            }
        }

        public asd.RectangleShape CollisionShape { get; set; }

        public MapObject()
        {
            CameraGroup = 1;
            CollisionShape = new asd.RectangleShape();
            Color = new asd.Color(255, 255, 255, 200);
            DrawingPriority = 2;
            Texture = TextureManager.LoadTexture("walk_g0000.png");
            ScriptPath = "";
        }

        public MapObject(MapObjectIO mapObject)
        {
            CameraGroup = 1;
            CollisionShape = new asd.RectangleShape();
            Color = new asd.Color(255, 255, 255, 200);
            DrawingPriority = 2;
            ScriptPath = mapObject.ScriptPath;
            Position = mapObject.Position;
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

        public static explicit operator MapObjectIO(MapObject mapObject)
        {
            var result = new MapObjectIO()
            {
                ScriptPath = mapObject.ScriptPath,
                Position = mapObject.Position,
            };
            return result;
        }
    }
}
