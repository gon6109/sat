using SatScript.Damage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game
{
    /// <summary>
    /// ダメージ情報
    /// </summary>
    class DamageInfo : IDamage
    {
        /// <summary>
        /// ダメージ量
        /// </summary>
        public int RecieveDamage { get; }

        /// <summary>
        /// ノックバック率
        /// </summary>
        public float KnockBack { get; }

        /// <summary>
        /// ダウン時間
        /// </summary>
        public int TakeDown { get; }

        public DamageInfo(int recieveDamage, float knockBack, int takeDown)
        {
            RecieveDamage = recieveDamage;
            KnockBack = knockBack;
            TakeDown = takeDown;
        }
    }
}
