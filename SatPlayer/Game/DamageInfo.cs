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
    public class DamageInfo : IDamage
    {
        /// <summary>
        /// ダメージ量
        /// </summary>
        public int RecieveDamage { get; set; }

        /// <summary>
        /// ノックバック率
        /// </summary>
        public float KnockBack { get; set; }

        /// <summary>
        /// ダウン時間
        /// </summary>
        public int TakeDown { get; set; }

        /// <summary>
        /// 優先度
        /// </summary>
        public int Priority { get; set; }

        public DamageInfo(int recieveDamage, float knockBack, int takeDown, int priority)
        {
            RecieveDamage = recieveDamage;
            KnockBack = knockBack;
            TakeDown = takeDown;
            Priority = priority;
        }
    }
}
