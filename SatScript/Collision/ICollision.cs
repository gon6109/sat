using System;
using System.Collections.Generic;
using System.Text;

namespace SatScript.Collision
{
    public interface ICollision
    {
        /// <summary>
        /// 壁・床と衝突しているか
        /// </summary>
        bool IsCollidedWithObstacle { get; }

        /// <summary>
        /// プレイヤーと衝突しているか
        /// </summary>
        bool IsCollidedWithPlayer { get; }

        /// <summary>
        /// 衝突しているMapObject/Eventobjectのタグ
        /// </summary>
        IEnumerable<string> ColligingMapObjectTags { get; }
    }
}
