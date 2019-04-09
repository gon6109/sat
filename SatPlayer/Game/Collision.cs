using SatScript.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game
{
    /// <summary>
    /// 衝突情報
    /// </summary>
    public class Collision : ICollision
    {
        /// <summary>
        /// 障害物と衝突したか
        /// </summary>
        public bool IsCollidedWithObstacle { get; set; }

        /// <summary>
        /// プレイヤーと衝突したか
        /// </summary>
        public bool IsCollidedWithPlayer { get; set; }

        /// <summary>
        /// 衝突しているMapObjectのTag
        /// </summary>
        public List<string> ColligingMapObjectTags { get; set; }

        /// <summary>
        /// スクリプト用ColligingMapObjectTags
        /// </summary>
        IEnumerable<string> ICollision.ColligingMapObjectTags => ColligingMapObjectTags;
    }
}
