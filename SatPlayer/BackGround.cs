using BaseComponent;
using SatIO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SatScript.BackGround;
using AltseedScript.Common;

namespace SatPlayer
{
    /// <summary>
    /// 背景
    /// </summary>
    public class BackGround : MultiAnimationObject2D, IBackGround
    {
        public asd.CameraObject2D Camera { get; private set; }

        protected MainMapLayer2D mainMap;

        public float Zoom { get; set; }

        /// <summary>
        /// OnUpdade時に呼び出される関数のデリゲート
        /// </summary>
        public Action<IBackGround> Update { get; set; } = obj => { };

        Vector IBackGround.Position
        {
            get => Position.ToScriptVector();
            set => Position = value.ToAsdVector();
        }

        public BackGround(MainMapLayer2D layer)
        {
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
            if (mainMap != null)
            {
                Camera.Dst = mainMap.PlayerCamera.Dst;
                Camera.Src = new asd.RectI((mainMap.PlayerCamera.Src.Position.To2DF() * Zoom).To2DI(), mainMap.PlayerCamera.Src.Size);
            }
            Update(this);
            base.OnUpdate();
        }

        public static BackGround LoadBackGroud(BackGroundIO backGroundIO, MainMapLayer2D layer)
        {
            BackGround backGround = new BackGround(layer);
            backGround.Position = backGroundIO.Position;
            backGround.Zoom = backGroundIO.Zoom;
            if (backGroundIO.TexturePath.IndexOf(".bg") > -1)
            {
                try
                {
                    using (var stream = IO.GetStream(backGroundIO.TexturePath))
                    {
                        var script = SatPlayer.ScriptOption.ScriptOptions["BackGround"].CreateScript<object>(stream.ToString());
                        var task = script.RunAsync(backGround);
                        task.Wait();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
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
