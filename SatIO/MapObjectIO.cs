using System;
using System.Collections.Generic;
using System.Text;

namespace SatIO
{
    [Serializable()]
    public class MapObjectIO
    {
        public VectorIO Position;

        /// <summary>
        /// 画像へのパス
        /// </summary>
        public string TexturePath;

        /// <summary>
        /// スクリプトへのパス
        /// </summary>
        public string ScriptPath;
    }
}
