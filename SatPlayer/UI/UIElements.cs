using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.UI
{
    /// <summary>
    /// UIを構成する要素の基底クラス
    /// </summary>
    public class UIElement : asd.TextureObject2D
    {
        /// <summary>
        /// 選択されているか
        /// </summary>
        public virtual bool IsSelected { get; set; }

        /// <summary>
        /// フォーカスされているか
        /// </summary>
        public virtual bool IsFocused { get; set; }

        /// <summary>
        /// 有効か否か
        /// </summary>
        public virtual bool IsEnable { get; set; }

        /// <summary>
        /// 上への参照
        /// </summary>
        public UIElement Up { get; set; }

        /// <summary>
        /// 下への参照
        /// </summary>
        public UIElement Down { get; set; }

        /// <summary>
        /// 右への参照
        /// </summary>
        public UIElement Right { get; set; }

        /// <summary>
        /// 左への参照
        /// </summary>
        public UIElement Left { get; set; }

        /// <summary>
        /// 要素のテクスチャ
        /// </summary>
        public new asd.Texture2D Texture
        {
            get => base.Texture;
            set
            {
                base.Texture = value;
                CenterPosition = value.Size.To2DF() / 2;
            }
        }

        /// <summary>
        /// コンストラクタ(初期化)
        /// </summary>
        public UIElement()
        {
        }

        /// <summary>
        /// 接続を消去
        /// </summary>
        public void ResetConnection()
        {
            Up = null;
            Down = null;
            Left = null;
            Right = null;
        }
    }
}
