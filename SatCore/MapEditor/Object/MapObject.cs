﻿using System;
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
using InspectorModel;

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

        [RootPathBinding("root")]
        public string RootPath => Config.Instance.RootPath;

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
        [FileInput("スクリプト", "MapObject File|*.mobj|All File|*.*", "root")]
        public string ScriptPath
        {
            get => _scriptPath;
            set
            {
                var task = LoadAnimationAsync(value, false);
                if (!task.Result)
                    return;
                UndoRedoManager.ChangeProperty(this, value);
                _scriptPath = value;
                OnPropertyChanged();
            }
        }

        private async Task<bool> LoadAnimationAsync(string path, bool awaitable = true)
        {
            var result = true;
            if (path != null && asd.Engine.File.Exists(path))
            {
                AnimationPart.Clear();
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
                        Script<object> script = CSharpScript.Create(code, options: options, globalsType: typeof(MapObject));
                        await script.RunAsync(this).ConfigureAwait(awaitable);
                        foreach (var item in LoadTextureTasks)
                        {
                            if (awaitable)
                                await AddAnimationPartAsync(item.animationGroup, item.extension, item.sheets, item.partName, item.interval);
                            else
                                base.AddAnimationPart(item.animationGroup, item.extension, item.sheets, item.partName, item.interval);
                        }
                        LoadTextureTasks.Clear();
                        State = AnimationPart.First().Key;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    Texture = TextureManager.LoadTexture("Static/error.png");
                }
            }
            else
            {
                Texture = TextureManager.LoadTexture("Static/error.png");
                result = false;
            }
            CenterPosition = Texture.Size.To2DF() / 2;
            CollisionShape.DrawingArea = new asd.RectF(Position - CenterPosition, Texture.Size.To2DF());
            return result;
        }

        public asd.RectangleShape CollisionShape { get; }

        public asd.Vector2DF BottomRight => Position + CenterPosition;

        protected List<(string animationGroup, string extension, int sheets, string partName, int interval)> LoadTextureTasks { get; } = new List<(string animationGroup, string extension, int sheets, string partName, int interval)>();

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

        public new void AddAnimationPart(string animationGroup, string extension, int sheets, string partName, int interval)
        {
            LoadTextureTasks.Add((animationGroup, extension, sheets, partName, interval));
        }

        public static async Task<MapObject> CreateMapObjectAsync(MapObjectIO mapObjectIO)
        {
            var mapObject = new MapObject();
            mapObject.Color = new asd.Color(255, 255, 255, 200);
            mapObject.DrawingPriority = 2;
            if (await mapObject.LoadAnimationAsync(mapObjectIO.ScriptPath))
                mapObject._scriptPath = mapObjectIO.ScriptPath;
            mapObject.CenterPosition = mapObject.Texture.Size.To2DF() / 2;
            mapObject.CollisionShape.DrawingArea = new asd.RectF(mapObject.Position - mapObject.CenterPosition, mapObject.Texture.Size.To2DF());
            mapObject.Position = mapObjectIO.Position;
            return mapObject;
        }
    }
}
