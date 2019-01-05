using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor
{
    /// <summary>
    /// 移動可能オブジェクトインターフェース
    /// </summary>
    public interface IMovable
    {
        void StartMove();
        void EndMove();
    }
}
