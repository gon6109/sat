using SatPlayer.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game
{
    /// <summary>
    /// エフェクト発生オブジェクトインターフェース
    /// </summary>
    public interface IEffectManeger
    {
        /// <summary>
        /// エフェクト
        /// </summary>
        Dictionary<string, object> Effects { get; }

        /// <summary>
        /// アニメーションエフェクトを読み込む
        /// </summary>
        /// <param name="animationGroup">エフェクトへのパス</param>
        /// <param name="extension">拡張子</param>
        /// <param name="sheets">枚数</param>
        /// <param name="name">エフェクト名</param>
        /// <param name="interval">1コマあたりのフレーム数</param>
        void LoadEffect(string animationGroup, string extension, int sheets, string name, int interval);

        /// <summary>
        /// Effekseerエフェクトを読み込む
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        void LoadEffect(string path, string name);

        /// <summary>
        /// エフェクトを配置する
        /// </summary>
        /// <param name="name">エフェクト名</param>
        /// <param name="position">座標</param>
        void SetEffect(string name, asd.Vector2DF position);
    }
}
