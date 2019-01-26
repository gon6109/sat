using AltseedScript.Common;
using SatScript.Collision;
using System;
using System.Collections.Generic;
using System.Text;

namespace SatScript.Player
{
    /// <summary>
    /// Playerスクリプト用インターフェース
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// キャラクター名
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 座標
        /// </summary>
        Vector Position { get; set; }

        /// <summary>
        /// HP初期値100
        /// </summary>
        int HP { get; set; }

        /// <summary>
        /// アニメーション状態
        /// </summary>
        string State { get; set; }

        /// <summary>
        /// 衝突情報
        /// </summary>
        ICollision Collision { get; }

        /// <summary>
        /// アニメーションパートを追加する
        /// </summary>
        /// <param name="animationGroup">ファイル名</param>
        /// <param name="extension">拡張子</param>
        /// <param name="sheets">枚数</param>
        /// <param name="partName">パート名</param>
        /// <param name="interval">1コマ当たりのフレーム数</param>
        void AddAnimationPart(string animationGroup, string extension, int sheets, string partName, int interval);

        /// <summary>
        /// 地面と接しているか
        /// </summary>
        bool IsColligedWithGround { get; }

        /// <summary>
        /// 速度
        /// </summary>
        Vector Velocity { get; set; }

        /// <summary>
        /// OnUpdate時に呼び出される関数のデリゲート
        /// </summary>
        Action<IPlayer> Update { get; set; }

        /// <summary>
        /// 入力状態を取得する(Event時対応)
        /// </summary>
        /// <param name="inputs">ボタン</param>
        /// <returns>何フレーム押されているか</returns>
        int GetInputState(Inputs inputs);

        /// <summary>
        /// エフェクトをロードする
        /// </summary>
        /// <param name="animationGroup">ファイル名</param>
        /// <param name="extension">拡張子</param>
        /// <param name="sheets">枚数</param>
        /// <param name="name">エフェクト名</param>
        /// <param name="interval">1コマ当たりのフレーム数</param>
        void LoadEffect(string animationGroup, string extension, int sheets, string name, int interval);

        /// <summary>
        /// エフェクトを配置する
        /// </summary>
        /// <param name="name">エフェクト名</param>
        /// <param name="positon">相対座標</param>
        void SetEffect(string name, Vector positon);

        /// <summary>
        /// イベント時か
        /// </summary>
        bool IsEvent { get; }
    }
}
