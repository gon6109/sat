using System;
using System.Collections.Generic;
using System.Text;

namespace SatIO
{
    [Serializable()]
    public class MapObjectIO
    {
        public VectorIO Position { get; set; }

        /// <summary>
        /// 画像へのパス
        /// </summary>
        public string TexturePath { get; set; }

        /// <summary>
        /// スクリプトへのパス
        /// </summary>
        public string ScriptPath { get; set; }
    }
}
