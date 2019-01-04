using BaseComponent;
using SatIO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    public class BackGround : MultiAnimationObject2D
    {
        static ScriptOptions options = ScriptOptions.Default.WithImports("SatPlayer", "System", "System.Collections.Generic")
                                                         .WithReferences(System.Reflection.Assembly.GetAssembly(typeof(IEnumerator<>))
                                                                         , System.Reflection.Assembly.GetAssembly(typeof(asd.Vector2DF))
                                                                         , System.Reflection.Assembly.GetAssembly(typeof(BackGround)));
        
        public asd.CameraObject2D Camera { get; private set; }

        MainMapLayer2D mainMap;

        public Action<BackGround> Update { get; set; }

        public float Zoom { get; set; }

        public BackGround(MainMapLayer2D layer)
        {
            Update = (back) => { };
            Camera = new asd.CameraObject2D();
            mainMap = layer;
            Zoom = 1;
            Camera.UpdatePriority = 10;
            DrawingPriority = -1;
        }

        protected override void OnAdded()
        {
            CameraGroup = (int)Math.Pow(2, Layer.Objects.Count(obj => obj is BackGround) + 1);
            Camera.CameraGroup = CameraGroup;
            Layer.AddObject(Camera);
            base.OnAdded();
        }

        protected override void OnUpdate()
        {
            Camera.Dst = mainMap.PlayerCamera.Dst;
            Camera.Src = new asd.RectI((mainMap.PlayerCamera.Src.Position.To2DF() * Zoom).To2DI(), mainMap.PlayerCamera.Src.Size);

            Update(this);
            base.OnUpdate();
        }

        public static BackGround LoadBackGroud(BackGroundIO backGroundIO, MainMapLayer2D layer)
        {
            BackGround backGround = new BackGround(layer);
            backGround.Position = backGroundIO.Position;
            backGround.Zoom = backGroundIO.Zoom;
            if (backGroundIO.TexturePath.IndexOf(".csx") > -1)
            {
                try
                {
                    var script = CSharpScript.Create(IO.GetStream(backGroundIO.TexturePath), options: options, globalsType: typeof(BackGround));
                    var task = script.RunAsync(backGround);
                    task.Wait();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
            }
            else backGround.Texture = TextureManager.LoadTexture(backGroundIO.TexturePath);
            if (backGround.Zoom > 1) backGround.Camera.DrawingPriority = 3;
            else backGround.Camera.DrawingPriority = -1;
            return backGround;
        }
    }
}
