using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor.Object
{
    /// <summary>
    /// マップ要素インターフェース
    /// </summary>
    interface IMapElement
    {
        asd.Vector2DF BottomRight { get; }
    }
}
