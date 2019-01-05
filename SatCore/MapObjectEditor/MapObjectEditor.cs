using BaseComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapObjectEditor
{
    /// <summary>
    /// マップオブジェクト編集シーン
    /// </summary>
    public class MapObjectEditor : UndoRedoScene
    {
        public EditableMapObject MapObject { get; private set; }

        PhysicAltseed.PhysicalWorld PhysicalWorld { get; set; }
        MainMapLayer2D MainLayer { get; set; }
        asd.CameraObject2D MainCamera { get; set; }

        public string Path { get; set; }

        [Button("オブジェクトをクリア")]
        public void ClearMapObject()
        {
            foreach (var item in MainLayer.Objects.Where(obj => obj is SatPlayer.MapObject))
            {
                item.Dispose();
            }
        }

        public MapObjectEditor(string path = "")
        {
            Path = path;
            MainLayer = new MainMapLayer2D();
            PhysicalWorld = MainLayer.PhysicalWorld;

            asd.RectF[] rects = { new asd.RectF(0, 30, 30, 1080), new asd.RectF(30, 1050, 1890, 30), new asd.RectF(1890, 0, 30, 1050), new asd.RectF(0, 0, 1890, 30) };
            foreach (var item in rects)
            {
                var groundShape = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Static, PhysicalWorld);
                groundShape.DrawingArea = item;
                MainLayer.AddObject(new asd.GeometryObject2D() { Shape = groundShape, Color = new asd.Color(255, 255, 255) });
                MainLayer.CollisionShapes.Add(groundShape);
            }

            MapObject = new EditableMapObject(PhysicalWorld);
            try
            {
                MapObject.Code = Encoding.UTF8.GetString(IO.GetStream(path).ToArray());
            }
            catch
            {
                MapObject.Code = "";
            }
            MapObject.Run();

            MainCamera = new asd.CameraObject2D();
            MainCamera.Src = new asd.RectI(new asd.Vector2DI(), Base.ScreenSize);
            if ((float)asd.Engine.WindowSize.X / asd.Engine.WindowSize.Y >= (float)Base.ScreenSize.X / Base.ScreenSize.Y)
                MainCamera.Dst = new asd.RectI((asd.Engine.WindowSize.X - Base.ScreenSize.X * asd.Engine.WindowSize.Y / Base.ScreenSize.Y) / 2, 0, Base.ScreenSize.X * asd.Engine.WindowSize.Y / Base.ScreenSize.Y, asd.Engine.WindowSize.Y);
            else MainCamera.Dst = new asd.RectI(0, (asd.Engine.WindowSize.Y - Base.ScreenSize.Y * asd.Engine.WindowSize.X / Base.ScreenSize.X) / 2, asd.Engine.WindowSize.X, Base.ScreenSize.Y * asd.Engine.WindowSize.X / Base.ScreenSize.X);
            MainLayer.AddObject(MainCamera);
            AddLayer(MainLayer);
        }

        protected override void OnUpdating()
        {
            PhysicalWorld.Update();
            base.OnUpdating();
        }

        protected override void OnUpdated()
        {
            if (Mouse.LeftButton == asd.ButtonState.Push && MapObject.IsSuccessBuild)
            {
                SatPlayer.MapObject obj = (SatPlayer.MapObject)MapObject.Clone();
                obj.Position = Mouse.Position;
                MainLayer.AddObject(obj);
            }

            foreach (asd.GeometryObject2D item in MainLayer.Objects.Where(obj => obj is asd.GeometryObject2D))
            {
                item.Color = MapObject.IsSuccessBuild ? new asd.Color(0, 255, 0) : new asd.Color(255, 0, 0);
            }

            if ((float)asd.Engine.WindowSize.X / asd.Engine.WindowSize.Y >= (float)Base.ScreenSize.X / Base.ScreenSize.Y)
                MainCamera.Dst = new asd.RectI((asd.Engine.WindowSize.X - Base.ScreenSize.X * asd.Engine.WindowSize.Y / Base.ScreenSize.Y) / 2, 0, Base.ScreenSize.X * asd.Engine.WindowSize.Y / Base.ScreenSize.Y, asd.Engine.WindowSize.Y);
            else MainCamera.Dst = new asd.RectI(0, (asd.Engine.WindowSize.Y - Base.ScreenSize.Y * asd.Engine.WindowSize.X / Base.ScreenSize.X) / 2, asd.Engine.WindowSize.X, Base.ScreenSize.Y * asd.Engine.WindowSize.X / Base.ScreenSize.X);
            base.OnUpdated();
        }

        public void SaveMapObject(string path)
        {
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(MapObject.Code);
            writer.Close();
        }
    }
}
