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
        /// 色
        /// </summary>
        Color Color { get; set; }

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
        bool IsCollidedWithGround { get; }

        /// <summary>
        /// 速度
        /// </summary>
        Vector Velocity { get; set; }

        /// <summary>
        /// 力を加える
        /// </summary>
        /// <param name="direct">力の強さ・向き</param>
        /// <param name="position">力を加える場所の相対座標</param>
        void SetForce(Vector direct, Vector position);

        /// <summary>
        /// 衝撃を加える
        /// </summary>
        /// <param name="direct">衝撃の強さ・向き</param>
        /// <param name="position">衝撃を加える場所の相対座標</param>
        void SetImpulse(Vector direct, Vector position);

        /// <summary>
        /// OnUpdate時に呼び出される関数のデリゲート
        /// </summary>
        event Action<IPlayer> Update;

        /// <summary>
        /// 入力状態を取得する(Event時対応)
        /// </summary>
        /// <param name="inputs">ボタン</param>
        /// <returns>何フレーム押されているか</returns>
        int GetInputState(Inputs inputs);

        /// <summary>
        /// アニメーションエフェクトをロードする
        /// </summary>
        /// <param name="animationGroup">ファイル名</param>
        /// <param name="extension">拡張子</param>
        /// <param name="sheets">枚数</param>
        /// <param name="name">エフェクト名</param>
        /// <param name="interval">1枚当たりのフレーム数</param>
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
        /// <param name="positon">相対座標</param>
        void SetEffect(string name, Vector positon);

        /// <summary>
        /// イベント時か
        /// </summary>
        bool IsEvent { get; }
    }
}
