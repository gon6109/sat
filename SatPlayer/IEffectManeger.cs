using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    /// <summary>
    /// エフェクト発生オブジェクトインターフェース
    /// </summary>
    public interface IEffectManeger
    {
        /// <summary>
        /// 再生中のエフェクト
        /// </summary>
        Dictionary<string, Effect> Effects { get; }

        /// <summary>
        /// エフェクトを読み込む
        /// </summary>
        /// <param name="animationGroup">エフェクトへのパス</param>
        /// <param name="extension">拡張子</param>
        /// <param name="sheets">枚数</param>
        /// <param name="name">エフェクト名</param>
        /// <param name="interval">1コマあたりのフレーム数</param>
        void LoadEffect(string animationGroup, string extension, int sheets, string name, int interval);

        /// <summary>
        /// エフェクトを配置する
        /// </summary>
        /// <param name="name">エフェクト名</param>
        /// <param name="position">座標</param>
        void SetEffect(string name, asd.Vector2DF position);
    }
}
