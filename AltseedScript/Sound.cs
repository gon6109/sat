using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlteseedScript.Common
{
    /// <summary>
    /// 音源(BaseComponent.Soundのラッパー変更箇所は今のところなし)
    /// </summary>
    public class Sound : BaseComponent.Sound
    {
        public Sound(string path, bool isMultiplePlay = true, bool isDecompressed = false) : base(path, isMultiplePlay, isDecompressed)
        {
        }
    }
}
