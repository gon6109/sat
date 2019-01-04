using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.CharacterImageEditor
{
    public class CharacterImageEditor : UndoRedoScene
    {
        static asd.Vector2DI ImageSize = new asd.Vector2DI(400, 800);

        public CharacterImage Character { get; private set; }
        asd.Layer2D MainLayer { get; set; }
        asd.CameraObject2D MainCamera { get; set; }
        public string Path { get; set; }

        public CharacterImageEditor(string path = "")
        {
            Path = path;
            MainLayer = new asd.Layer2D();

            Character = path == "" ? new CharacterImage() :  new CharacterImage(path);
            MainLayer.AddObject(Character);

            MainCamera = new asd.CameraObject2D();
            MainCamera.Src = new asd.RectI(new asd.Vector2DI(), ImageSize);
            if ((float)asd.Engine.WindowSize.X / asd.Engine.WindowSize.Y >= 1.0f / 2)
                MainCamera.Dst = new asd.RectI((asd.Engine.WindowSize.X - ImageSize.X * asd.Engine.WindowSize.Y / ImageSize.Y) / 2, 0, ImageSize.X * asd.Engine.WindowSize.Y / ImageSize.Y, asd.Engine.WindowSize.Y);
            else MainCamera.Dst = new asd.RectI(0, (asd.Engine.WindowSize.Y - ImageSize.Y * asd.Engine.WindowSize.X / ImageSize.X) / 2, asd.Engine.WindowSize.X, ImageSize.Y * asd.Engine.WindowSize.X / ImageSize.X);
            MainLayer.AddObject(MainCamera);
            AddLayer(MainLayer);
        }

        protected override void OnUpdating()
        {
            base.OnUpdating();
        }

        protected override void OnUpdated()
        {
            if ((float)asd.Engine.WindowSize.X / asd.Engine.WindowSize.Y >= 1.0f / 2)
                MainCamera.Dst = new asd.RectI((asd.Engine.WindowSize.X - ImageSize.X * asd.Engine.WindowSize.Y / ImageSize.Y) / 2, 0, ImageSize.X * asd.Engine.WindowSize.Y / ImageSize.Y, asd.Engine.WindowSize.Y);
            else MainCamera.Dst = new asd.RectI(0, (asd.Engine.WindowSize.Y - ImageSize.Y * asd.Engine.WindowSize.X / ImageSize.X) / 2, asd.Engine.WindowSize.X, ImageSize.Y * asd.Engine.WindowSize.X / ImageSize.X);
            base.OnUpdated();
        }

        public void SaveCharacterImage(string path)
        {
            ((SatIO.MapEventIO.CharacterImageIO)Character).SaveCharacterImageIO(path);
        }
    }
}
