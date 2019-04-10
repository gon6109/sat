using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game
{
    /// <summary>
    /// エフェクト
    /// </summary>
    public class Effect : AnimationObject2D, ICloneable
    {
        public Effect()
        {
            CameraGroup = 1;
        }

        private new void LoadAnimationFile(string animationGroup, string extension, int sheets)
        {
            base.LoadAnimationFile(animationGroup, extension, sheets);
        }

        /// <summary>
        /// エフェクトを読み込む
        /// </summary>
        ///<param name="animationGroup">ファイル名</param>
        ///<param name="extension">拡張子</param>
        ///<param name="sheets">枚数</param>
        /// <param name="interval">1枚あたりのフレーム数</param>
        public void LoadEffect(string animationGroup, string extension, int sheets, int interval)
        {
            LoadAnimationFile(animationGroup, extension, sheets);
            IsOneLoop = true;
            Interval = interval;
            DrawingPriority = 3;
        }

        /// <summary>
        /// インスタンスの複製
        /// </summary>
        /// <returns>複製されたインスタンス</returns>
        public new object Clone()
        {
            Effect clone = new Effect();
            clone._textures = new List<asd.Texture2D>(_textures);
            clone.IsOneLoop = IsOneLoop;
            clone.Interval = Interval;
            clone.Texture = Texture;
            clone.CenterPosition = _textures.Count > 0 ? _textures.First().Size.To2DF() / 2 : new asd.Vector2DF();
            clone.DrawingPriority = DrawingPriority;
            return clone;
        }
    }
}
