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
using SatCore.Attribute;
using asd;

namespace SatCore.MapEditor.Object
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _texturePath;
        private float _zoom;
        private bool _isMove;
        asd.Vector2DF prePosition;
        private Vector2DF _position;

        List<Task> LoadTextureTasks { get; }

        /// <summary>
        /// 動かすか
        /// </summary>
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

        [FileInput("背景画像/Animation", "Readable File|*.png;*.bg|PNG File|*.png|Back Ground Script|*.bg")]
        public string TexturePath
        {
            get => _texturePath;
            set
            {
                if (!asd.Engine.File.Exists(value)) return;
                var task = LoadTextureAsync(value, false);
                if (!task.Result)
                    return;
                UndoRedoManager.ChangeProperty(this, value);
                _texturePath = value;
                OnPropertyChanged();
                OnPropertyChanged("Name");
            }
        }

        private async Task<bool> LoadTextureAsync(string path, bool awaitable = true)
        {
            if (path.IndexOf(".png") > -1)
            {
                if (awaitable)
                    Texture = await TextureManager.LoadTextureAsync(path);
                else
                    Texture = TextureManager.LoadTexture(path);

            }
            else if (path.IndexOf(".bg") > -1)
            {
                try
                {
                    using (var stream = awaitable ? await IO.GetStreamAsync(path) : IO.GetStream(path))
                    using (var reader = new StreamReader(stream))
                    {
                        string code = "";
                        string temp;
                        while ((temp = reader.ReadLine()) != null)
                        {
                            if (temp.IndexOf("AddAnimationPart(") > -1) code += temp + "\n";
                        }
                        Script<object> script = CSharpScript.Create(code, options: options, globalsType: typeof(BackGround));
                        await script.RunAsync(this).ConfigureAwait(awaitable);
                        LoadTextureTasks.Clear();
                        State = AnimationPart.First().Key;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            else
                return false;
            return true;
        }

        [VectorInput("座標")]
        public new asd.Vector2DF Position
        {
            get => _position;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _position = value;
                OnPropertyChanged();
            }
        }

        [TextInput("移動率", false)]
        public float Zoom
        {
            get => _zoom;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _zoom = value;
                if (value > 1)
                {
                    Color = new asd.Color(255, 255, 255, 210);
                }
                else
                {
                    Color = new asd.Color(255, 255, 255);
                }
                OnPropertyChanged();
            }
        }

        public string Name => TexturePath;

        [Button("カーソルキーで移動")]
        public void Move()
        {
            IsMove = !IsMove;
        }

        public BackGround()
        {
            LoadTextureTasks = new List<Task>();
            _zoom = 1;
            UpdatePriority = 10;
            DrawingPriority = -1;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
        }

        protected override void OnUpdate()
        {
            if (Layer is MapLayer map)
            {
                base.Position = Position - map.ScrollCamera.Src.Position.To2DF() * (Zoom - 1);
            }

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

        public new void AddAnimationPart(string animationGroup, string extension, int sheets, string partName, int interval)
        {
            LoadTextureTasks.Add(AddAnimationPartAsync(animationGroup, extension, sheets, partName, interval));
        }

        public ICopyPasteObject Copy()
        {
            BackGround copy = new BackGround();
            copy.Position = Position + new asd.Vector2DF(50, 50);
            copy.TexturePath = TexturePath;
            copy.Zoom = Zoom;
            return copy;
        }

        public BackGroundIO ToIO()
        {
            BackGroundIO backGroundIO = new BackGroundIO()
            {
                Position = Position,
                Zoom = Zoom,
                TexturePath = TexturePath,
            };
            return backGroundIO;
        }

        public static async Task<BackGround> CreateBackGroudAsync(BackGroundIO backGroundIO)
        {
            BackGround backGround = new BackGround();
            backGround.Position = backGroundIO.Position;
            backGround.Zoom = backGroundIO.Zoom;
            if (await backGround.LoadTextureAsync(backGroundIO.TexturePath))
                backGround._texturePath = backGroundIO.TexturePath;
            return backGround;
        }
    }
}
