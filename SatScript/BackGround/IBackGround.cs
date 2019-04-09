using AltseedScript.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SatScript.BackGround
{
    /// <summary>
    /// BackGroundスクリプト用インターフェース
    /// </summary>
    public interface IBackGround
    {
        /// <summary>
        /// 座標
        /// </summary>
        Vector Position { get; set; }

        /// <summary>
        /// アニメーション状態
        /// </summary>
        string State { get; set; }

        /// <summary>
        /// 色
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// OnUpdate時に呼び出される関数のデリゲート
        /// </summary>
        event Action<IBackGround> Update;

        /// <summary>
        /// アニメーションパートを追加する
        /// </summary>
        /// <param name="animationGroup">ファイル名</param>
        /// <param name="extension">拡張子</param>
        /// <param name="sheets">枚数</param>
        /// <param name="partName">パート名</param>
        /// <param name="interval">1コマあたりのフレーム数</param>
        void AddAnimationPart(string animationGroup, string extension, int sheets, string partName, int interval);
    }
}
