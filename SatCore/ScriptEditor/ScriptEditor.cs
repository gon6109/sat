using BaseComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.ScriptEditor
{
    /// <summary>
    /// スクリプト編集シーン
    /// </summary>
    public class ScriptEditor : UndoRedoScene
    {
        public IScriptObject ScriptObject { get; private set; }

        PhysicAltseed.PhysicalWorld PhysicalWorld { get; set; }
        MainMapLayer2D MainLayer { get; set; }
        asd.CameraObject2D MainCamera { get; set; }

        public ScriptType Script { get; }

        public string Path { get; set; }

        [Button("オブジェクトをクリア")]
        public void ClearMapObject()
        {
            foreach (var item in MainLayer.Objects.Where(obj => obj is SatPlayer.MapObject))
            {
                item.Dispose();
            }
        }

        public ScriptEditor(ScriptType scriptType ,string path = "")
        {
            Path = path;
            Script = scriptType;

            MainLayer = new MainMapLayer2D();
            PhysicalWorld = MainLayer.PhysicalWorld;

            CreateObject();
            try
            {
                ScriptObject.Code = Encoding.UTF8.GetString(IO.GetStream(path).ToArray());
            }
            catch
            {
                ScriptObject.Code = "";
            }
            ScriptObject.Run();

            MainLayer.IsPreparePlayer = ScriptObject.IsPreparePlayer;

            asd.RectF[] rects = { new asd.RectF(0, 30, 30, 1080), new asd.RectF(30, 1050, 1890, 30), new asd.RectF(1890, 0, 30, 1050), new asd.RectF(0, 0, 1890, 30) };
            foreach (var item in rects)
            {
                var groundShape = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Static, PhysicalWorld);
                groundShape.DrawingArea = item;
                MainLayer.AddObject(new asd.GeometryObject2D() { Shape = groundShape, Color = new asd.Color(255, 255, 255) });
                MainLayer.CollisionShapes.Add(groundShape);
            }

            if (ScriptObject.IsSingle && ScriptObject is asd.Object2D obj) MainLayer.AddObject(obj);

            MainCamera = new asd.CameraObject2D();
            MainCamera.Src = new asd.RectI(new asd.Vector2DI(), Base.ScreenSize);
            if ((float)asd.Engine.WindowSize.X / asd.Engine.WindowSize.Y >= (float)Base.ScreenSize.X / Base.ScreenSize.Y)
                MainCamera.Dst = new asd.RectI((asd.Engine.WindowSize.X - Base.ScreenSize.X * asd.Engine.WindowSize.Y / Base.ScreenSize.Y) / 2, 0, Base.ScreenSize.X * asd.Engine.WindowSize.Y / Base.ScreenSize.Y, asd.Engine.WindowSize.Y);
            else MainCamera.Dst = new asd.RectI(0, (asd.Engine.WindowSize.Y - Base.ScreenSize.Y * asd.Engine.WindowSize.X / Base.ScreenSize.X) / 2, asd.Engine.WindowSize.X, Base.ScreenSize.Y * asd.Engine.WindowSize.X / Base.ScreenSize.X);
            MainLayer.AddObject(MainCamera);
            AddLayer(MainLayer);
        }

        void CreateObject()
        {
            switch (Script)
            {
                case ScriptType.MapObject:
                    ScriptObject = new EditableMapObject(PhysicalWorld);
                    break;
                case ScriptType.EventObject:
                    ScriptObject = new EditableEventObject(PhysicalWorld);
                    break;
                case ScriptType.Player:
                    ScriptObject = new EditablePlayer(PhysicalWorld);
                    break;
                case ScriptType.BackGround:
                    ScriptObject = new EditableBackGround(MainLayer);
                    break;
                default:
                    throw new NotImplementedException(Script.ToString());
            }
        }

        protected override void OnUpdating()
        {
            PhysicalWorld.Update();
            base.OnUpdating();
        }

        protected override void OnUpdated()
        {
            if (Mouse.LeftButton == asd.ButtonState.Push && ScriptObject.IsSuccessBuild)
            {
                if (ScriptObject.Clone() is asd.Object2D obj)
                {
                    obj.Position = Mouse.Position;
                    MainLayer.AddObject(obj);
                }
            }

            foreach (asd.GeometryObject2D item in MainLayer.Objects.Where(obj => obj is asd.GeometryObject2D))
            {
                item.Color = ScriptObject.IsSuccessBuild ? new asd.Color(0, 255, 0) : new asd.Color(255, 0, 0);
            }

            if ((float)asd.Engine.WindowSize.X / asd.Engine.WindowSize.Y >= (float)Base.ScreenSize.X / Base.ScreenSize.Y)
                MainCamera.Dst = new asd.RectI((asd.Engine.WindowSize.X - Base.ScreenSize.X * asd.Engine.WindowSize.Y / Base.ScreenSize.Y) / 2, 0, Base.ScreenSize.X * asd.Engine.WindowSize.Y / Base.ScreenSize.Y, asd.Engine.WindowSize.Y);
            else MainCamera.Dst = new asd.RectI(0, (asd.Engine.WindowSize.Y - Base.ScreenSize.Y * asd.Engine.WindowSize.X / Base.ScreenSize.X) / 2, asd.Engine.WindowSize.X, Base.ScreenSize.Y * asd.Engine.WindowSize.X / Base.ScreenSize.X);
            base.OnUpdated();
        }

        public void SaveScript(string path)
        {
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(ScriptObject.Code);
            writer.Close();
        }

        public enum ScriptType
        {
            MapObject,
            EventObject,
            Player,
            BackGround,
        }
    }
}
