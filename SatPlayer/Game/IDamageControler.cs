using SatScript.Damage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game
{
    /// <summary>
    /// 攻撃をする・ダメージを受けるオブジェクト
    /// </summary>
    public interface IDamageControler
    {
        /// <summary>
        /// HP
        /// </summary>
        int HP { get; set; }

        /// <summary>
        /// ダメージを受けるか
        /// </summary>
        bool IsReceiveDamage { get; set; }

        /// <summary>
        /// ダメージ要求
        /// </summary>
        Queue<DamageRect> DamageRequests { get; }

        /// <summary>
        /// 直接攻撃要求
        /// </summary>
        Queue<DirectDamage> DirectDamageRequests { get; }

        /// <summary>
        /// 陣営
        /// </summary>
        int DamageGroup { get; }

        /// <summary>
        /// 当たり判定
        /// </summary>
        asd.Shape CollisionShape { get; }
    }
}
