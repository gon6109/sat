using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using asd;

namespace SatPlayer.Game
{
    /// <summary>
    /// 直接攻撃
    /// </summary>
    public class DirectDamage : DamageRect
    {
        public DirectDamage(IDamageControler recieveTo, int group, RectF rect, int damage, int frame, bool sastainable, int knockBack) : base(group, rect, damage, frame, sastainable, knockBack)
        {
            RecieveTo = recieveTo;
        }

        /// <summary>
        /// 対象
        /// </summary>
        public IDamageControler RecieveTo { get; }
    }
}
