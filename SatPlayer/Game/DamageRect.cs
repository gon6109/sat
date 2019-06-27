using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game
{
    /// <summary>
    /// ダメージ判定領域
    /// </summary>
    public class DamageRect : asd.RectangleShape
    {
        /// <summary>
        /// ダメージグループ
        /// </summary>
        public int Group { get; }

        /// <summary>
        /// 持続フレーム
        /// </summary>
        public int Frame { get; set; }

        /// <summary>
        /// 連続するか
        /// </summary>
        public bool Sastainable { get; }

        /// <summary>
        /// ダメージ量
        /// </summary>
        public int Damage { get; }

        /// <summary>
        /// ノックバック率
        /// </summary>
        public float KnockBack { get; }

        public DamageRect(int group, asd.RectF rect, int damage, int frame, bool sastainable, int knockBack)
        {
            Damage = damage;
            Group = group;
            DrawingArea = rect;
            Frame = frame;
            Sastainable = sastainable;
            KnockBack = knockBack;
        }
    }
}
