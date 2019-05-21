﻿using BaseComponent;
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
    public class BackGround : MultiAnimationObject2D, IBackGround, ICloneable
    {
        public float Zoom { get; set; }

        /// <summary>
        /// OnUpdade時に呼び出されるイベント
        /// </summary>
        public virtual event Action<IBackGround> Update = delegate { };

        /// <summary>
        /// スクリプト用Position
        /// </summary>
        Vector IBackGround.Position
        {
            get => Position.ToScriptVector();
            set => Position = value.ToAsdVector();
        }

        /// <summary>
        /// 座標
        /// 
        /// </summary>
        public new asd.Vector2DF Position { get; set; }

        /// <summary>
        /// スクリプト用Color
        /// </summary>
        Color IBackGround.Color { get => Color.ToScriptColor(); set => Color = value.ToAsdColor(); }

        public BackGround()
        {
            Zoom = 1;
            UpdatePriority = 10;
            DrawingPriority = -1;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
        }

        protected override void OnUpdate()
        {
            if (Layer is MapLayer mapLayer &&
                mapLayer.PlayerCamera != null)
            {
                base.Position = Position - mapLayer.PlayerCamera.Src.Position.To2DF() * (Zoom - 1);
            }
            Update(this);
            base.OnUpdate();
        }

        public new object Clone()
        {
            var clone = new BackGround();
            clone.Copy(this);
            clone.State = State;
            clone.Zoom = Zoom;
            clone.Update = Update;
            clone.UpdatePriority = UpdatePriority;
            return clone;
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
                    using (var stream = await IO.GetStreamAsync(backGroundIO.TexturePath))
                    {
                        var script = ScriptOption.ScriptOptions["BackGround"].CreateScript<object>(Encoding.UTF8.GetString(stream.ToArray()));
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
            if (backGround.Zoom > 1) backGround.DrawingPriority = 3;
            else backGround.DrawingPriority = -1;
            return backGround;
        }
    }
}
