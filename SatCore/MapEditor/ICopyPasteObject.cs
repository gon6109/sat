using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor
{
    /// <summary>
    /// コピーペースト可能オブジェクトインターフェース
    /// </summary>
    public interface ICopyPasteObject
    {
        ICopyPasteObject Copy();
    }
}
