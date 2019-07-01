using System;
using System.Collections.Generic;
using System.Text;

namespace SatScript.Damage
{
    /// <summary>
    /// ダメージ情報インターフェース
    /// </summary>
    public interface IDamage
    {
        /// <summary>
        /// 受けたダメージ量
        /// </summary>
        int RecieveDamage { get; }

        /// <summary>
        /// ノックバック率
        /// </summary>
        float KnockBack { get; }

        /// <summary>
        /// ダウン時間
        /// </summary>
        int TakeDown { get; }
    }
}
