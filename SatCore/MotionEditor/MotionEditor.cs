using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MotionEditor
{
    public class MotionEditor : UndoRedoScene
    {
        public Character Character { get; private set; }
        PhysicAltseed.PhysicalWorld PhysicalWorld { get; set; }
        asd.Layer2D MainLayer { get; set; }
        asd.CameraObject2D MainCamera { get; set; }
        public string Path { get; set; }

        public MotionEditor(string path = "", bool isEditPlayer = false)
        {
            Path = path;
            MainLayer = new asd.Layer2D();
            PhysicalWorld = new PhysicAltseed.PhysicalWorld(new asd.RectF(new asd.Vector2DF(), Base.ScreenSize.To2DF()), new asd.Vector2DF(0, 2000));

            asd.RectF[] rects = { new asd.RectF(0, 30, 30, 1080), new asd.RectF(30, 1050, 1890, 30), new asd.RectF(1890, 0, 30, 1050), new asd.RectF(0, 0, 1890, 30) };
            foreach (var item in rects)
            {
                var groundShape = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Static, PhysicalWorld);
                groundShape.DrawingArea = item;
                MainLayer.AddObject(new asd.GeometryObject2D() { Shape = groundShape, Color = new asd.Color(255, 255, 255) });
            }

            if (isEditPlayer) Character = path == "" ? new Player(PhysicalWorld) : new Player(path, PhysicalWorld);
            else Character = path == "" ? new Character(PhysicalWorld) : new Character(path, PhysicalWorld);
            Character.Position = new asd.Vector2DF(400, 400);
            MainLayer.AddObject(Character);

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
            if ((float)asd.Engine.WindowSize.X / asd.Engine.WindowSize.Y >= (float)Base.ScreenSize.X / Base.ScreenSize.Y)
                MainCamera.Dst = new asd.RectI((asd.Engine.WindowSize.X - Base.ScreenSize.X * asd.Engine.WindowSize.Y / Base.ScreenSize.Y) / 2, 0, Base.ScreenSize.X * asd.Engine.WindowSize.Y / Base.ScreenSize.Y, asd.Engine.WindowSize.Y);
            else MainCamera.Dst = new asd.RectI(0, (asd.Engine.WindowSize.Y - Base.ScreenSize.Y * asd.Engine.WindowSize.X / Base.ScreenSize.X) / 2, asd.Engine.WindowSize.X, Base.ScreenSize.Y * asd.Engine.WindowSize.X / Base.ScreenSize.X);
            base.OnUpdated();
        }

        public void SaveMotion(string path)
        {
            if (Character is Player) ((Player)Character).ToPlayerIO().SavePlayerIO(path);
            else Character.ToMotionIO().SaveMotionIO(path);
        }

        public void ImportMotionFile(string path)
        {
            Character.UpdateMotion(SatIO.MotionIO.GetMotionIO(path));
        }
    }
}
