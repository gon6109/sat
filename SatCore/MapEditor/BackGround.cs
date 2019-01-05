using BaseComponent;
using SatIO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor
{
    /// <summary>
    /// 背景
    /// </summary>
    public class BackGround : MultiAnimationObject2D, IListInput, ICopyPasteObject, INotifyPropertyChanged
    {
        static ScriptOptions options = ScriptOptions.Default.WithImports("SatPlayer", "System")
                                     .WithReferences(System.Reflection.Assembly.GetAssembly(typeof(MapObject))
                                                     , System.Reflection.Assembly.GetAssembly(typeof(asd.Vector2DF)));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public asd.CameraObject2D Camera { get; private set; }

        MainMapLayer2D mainMap;
        private string _texturePath;
        private float _zoom;
        private bool _isMove;
        asd.Vector2DF prePosition;

        public bool IsMove
        {
            get => _isMove;
            set
            {
                if (!value)
                {
                    UndoRedoManager.ChangeProperty(this, Position, prePosition, "Position");
                    OnPropertyChanged("Position");
                }
                else prePosition = Position;
                _isMove = value;
            }
        }

        [FileInput("背景画像/Animation", "Readable File|*.png;*.csx|PNG File|*.png|Script File|*.csx")]
        public string TexturePath
        {
            get => _texturePath;
            set
            {
                if (!asd.Engine.File.Exists(value)) return;
                if (value.IndexOf(".png") > -1)
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    _texturePath = value;
                    Texture = TextureManager.LoadTexture(value);
                    OnPropertyChanged();
                }
                else if (value.IndexOf(".csx") > -1)
                {
                    try
                    {
                        StreamReader reader = new StreamReader(IO.GetStream(value));
                        string code = "";
                        string temp;
                        while ((temp = reader.ReadLine()) != null)
                        {
                            if (temp.IndexOf("AddAnimationPart(") > -1) code += temp + "\n";
                        }
                        Script<object> script = CSharpScript.Create(code, options: options, globalsType: typeof(BackGround));
                        var thread = script.RunAsync(this);
                        thread.Wait();
                        State = AnimationPart.First().Key;
                        _texturePath = value;
                        OnPropertyChanged();
                    }
                    catch (Exception e)
                    {
                        ErrorIO.AddError(e);
                    }
                }
            }
        }

        [VectorInput("座標")]
        public new asd.Vector2DF Position
        {
            get => base.Position;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                base.Position = value;
                OnPropertyChanged();
            }
        }

        [TextInput("拡大率", false)]
        public float Zoom
        {
            get => _zoom;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                base.Position *= value / _zoom;
                _zoom = value;
                if (value > 1) Camera.DrawingPriority = 3;
                else Camera.DrawingPriority = -1;
                OnPropertyChanged();
            }
        }

        public string Name => TexturePath;

        [Button("カーソルキーで移動")]
        public void Move()
        {
            IsMove = !IsMove;
        }

        public BackGround(MainMapLayer2D layer)
        {
            Camera = new asd.CameraObject2D();
            mainMap = layer;
            Camera.Dst = mainMap.ScrollCamera.Dst;
            Camera.Src = new asd.RectI((mainMap.ScrollCamera.Src.Position.To2DF() * Zoom).To2DI(), mainMap.ScrollCamera.Src.Size);
            Position = (mainMap.ScrollCamera.Src.Position + mainMap.ScrollCamera.Src.Size).To2DF();
            _zoom = 1;
            Camera.UpdatePriority = 10;
            DrawingPriority = -1;
        }

        protected override void OnAdded()
        {
            CameraGroup = (int)Math.Pow(2, Layer.Objects.Count(obj => obj is BackGround) + 1);
            Camera.CameraGroup = CameraGroup;
            base.OnAdded();
        }

        protected override void OnUpdate()
        {
            Camera.Dst = mainMap.ScrollCamera.Dst;
            Camera.Src = new asd.RectI((mainMap.ScrollCamera.Src.Position.To2DF() * Zoom).To2DI(), mainMap.ScrollCamera.Src.Size);

            if (IsMove)
            {
                var _position = Position;
                if (Input.GetInputState(Inputs.B) > 0)
                {
                    if (Input.GetInputState(Inputs.Up) > 0) _position.Y -= 10;
                    if (Input.GetInputState(Inputs.Down) > 0) _position.Y += 10;
                    if (Input.GetInputState(Inputs.Left) > 0) _position.X -= 10;
                    if (Input.GetInputState(Inputs.Right) > 0) _position.X += 10;
                }
                else
                {
                    if (Input.GetInputState(Inputs.Up) > 0) _position.Y -= 5;
                    if (Input.GetInputState(Inputs.Down) > 0) _position.Y += 5;
                    if (Input.GetInputState(Inputs.Left) > 0) _position.X -= 5;
                    if (Input.GetInputState(Inputs.Right) > 0) _position.X += 5;
                }
                Position = _position;

                if (Input.GetInputState(Inputs.Esc) == 1
                    || Mouse.LeftButton == asd.ButtonState.Push) IsMove = false;
            }
            base.OnUpdate();
        }

        public static BackGround LoadBackGroud(BackGroundIO backGroundIO, MainMapLayer2D layer)
        {
            BackGround backGround = new BackGround(layer);
            backGround.Position = backGroundIO.Position;
            backGround._zoom = backGroundIO.Zoom;
            backGround.TexturePath = backGroundIO.TexturePath;
            if (backGround.Zoom > 1) backGround.Camera.DrawingPriority = 3;
            else backGround.Camera.DrawingPriority = -1;
            return backGround;
        }

        public ICopyPasteObject Copy()
        {
            BackGround copy = new BackGround(mainMap);
            copy.Position = Position + new asd.Vector2DF(50, 50);
            copy.TexturePath = TexturePath;
            copy.Zoom = Zoom;
            return copy;
        }

        public static explicit operator BackGroundIO(BackGround backGround)
        {
            BackGroundIO backGroundIO = new BackGroundIO()
            {
                Position = backGround.Position,
                Zoom = backGround.Zoom,
                TexturePath = backGround.TexturePath,
            };
            return backGroundIO;
        }
    }
}
