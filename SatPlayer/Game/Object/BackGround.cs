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

namespace SatPlayer.Game.Object
{
    /// <summary>
    /// 背景
    /// </summary>
    public class BackGround : MultiAnimationObject2D, IBackGround
    {
        /// <summary>
        /// 表示用カメラ
        /// </summary>
        public asd.CameraObject2D Camera { get; }

        public float Zoom { get; set; }

        /// <summary>
        /// OnUpdade時に呼び出されるイベント
        /// </summary>
        public Action<IBackGround> Update { get; set; } = delegate { };

        /// <summary>
        /// スクリプト用Position
        /// </summary>
        Vector IBackGround.Position
        {
            get => Position.ToScriptVector();
            set => Position = value.ToAsdVector();
        }

        /// <summary>
        /// スクリプト用Color
        /// </summary>
        Color IBackGround.Color { get => Color.ToScriptColor(); set => Color = value.ToAsdColor(); }

        public BackGround()
        {
            Camera = new asd.CameraObject2D();
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
            if (Layer is MapLayer mapLayer)
            {
                Camera.Dst = mapLayer.PlayerCamera.Dst;
                Camera.Src = new asd.RectI((mapLayer.PlayerCamera.Src.Position.To2DF() * Zoom).To2DI(), mapLayer.PlayerCamera.Src.Size);
            }
            Update(this);
            base.OnUpdate();
        }

        /// <summary>
        /// 背景情報から背景を作成する
        /// </summary>
        /// <param name="backGroundIO">背景情報</param>
        /// <returns>背景</returns>
        public static async Task<BackGround> CreateBackGroudAsync(BackGroundIO backGroundIO)
        {
            BackGround backGround = new BackGround();
            backGround.Position = backGroundIO.Position;
            backGround.Zoom = backGroundIO.Zoom;
            if (backGroundIO.TexturePath.IndexOf(".bg") > -1)
            {
                try
                {
                    var stream = await IO.GetStreamAsync(backGroundIO.TexturePath);
                    using (stream)
                    {
                        var script = ScriptOption.ScriptOptions["BackGround"].CreateScript<object>(stream.ToString());
                        await Task.Run(() => script.Compile());
                        await script.RunAsync(backGround);
                    }
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
            }
            else backGround.Texture = await TextureManager.LoadTextureAsync(backGroundIO.TexturePath);
            if (backGround.Zoom > 1) backGround.Camera.DrawingPriority = 3;
            else backGround.Camera.DrawingPriority = -1;
            return backGround;
        }
    }
}
