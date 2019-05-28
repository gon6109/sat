using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game.Object.MapEvent
{
    /// <summary>
    /// マップイベント共通リソース
    /// </summary>
    public class MapEventResource
    {
        static MapEventResource s_instance;

        /// <summary>
        /// インスタンスを取得
        /// </summary>
        public static MapEventResource Instance
        {
            get
            {
                if (s_instance == null)
                    if (asd.Engine.Graphics != null)
                        s_instance = new MapEventResource();
                    else
                        s_instance = null;
                return s_instance;
            }
        }

        /// <summary>
        /// メッセージボックス
        /// </summary>
        public asd.Texture2D MessageBoxTexture { get; set; }

        /// <summary>
        /// ネームボックス
        /// </summary>
        public asd.Texture2D NameBoxTexture { get; set; }

        /// <summary>
        /// メッセージフォント
        /// </summary>
        public asd.Font MessageFont { get; set; }

        /// <summary>
        /// テキストフォント
        /// </summary>
        public asd.Font NameFont { get; set; }

        private MapEventResource()
        {
            MessageBoxTexture = TextureManager.LoadTexture("Static/messageBox.png");
            NameBoxTexture = TextureManager.LoadTexture("Static/nameBox.png");
            MessageFont = asd.Engine.Graphics.CreateFont("Static/messageFont.aff");
            if (MessageFont == null)
            {
                Logger.Warning("MessageFont doesn't exist.");
                MessageFont = asd.Engine.Graphics.CreateDynamicFont("", 25, new asd.Color(255,255,255), 0, new asd.Color());
            }
            NameFont = asd.Engine.Graphics.CreateFont("Static/nameFont.aff");
            if (NameFont == null)
            {
                Logger.Warning("NameFont doesn't exist.");
                NameFont = asd.Engine.Graphics.CreateDynamicFont("", 30, new asd.Color(255, 255, 255), 0, new asd.Color());
            }
        }
    }
}
