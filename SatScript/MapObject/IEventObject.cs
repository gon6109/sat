using AltseedScript.Common;
using SatScript.Collision;
using System;
using System.Collections.Generic;
using System.Text;

namespace SatScript.MapObject
{
    /// <summary>
    /// EventObjectスクリプト用インターフェース
    /// </summary>
    public interface IEventObject
    {
        /// <summary>
        /// オブジェクトにつけられたユーザー定義のタグ
        /// </summary>
        string Tag { get; set; }

        /// <summary>
        /// 現在座標
        /// </summary>
        Vector Position { get; set; }

        /// <summary>
        /// HP(初期値100)
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
        /// アニメーションパートを追加する
        /// </summary>
        /// <param name="animationGroup">ファイル名</param>
        /// <param name="extension">拡張子</param>
        /// <param name="sheets">枚数</param>
        /// <param name="partName">パート名</param>
        /// <param name="interval">1枚当たりのフレーム数</param>
        void AddAnimationPart(string animationGroup, string extension, int sheets, string partName, int interval);

        /// <summary>
        /// MapObjectのタイプ
        /// </summary>
        MapObjectType MapObjectType { get; set; }

        /// <summary>
        /// 衝突情報
        /// </summary>
        ICollision Collision { get; }

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
        /// 衝突グループ
        /// </summary>
        short CollisionGroup { get; set; }

        /// <summary>
        /// 衝突カテゴリー
        /// </summary>
        ushort CollisionCategory { get; set; }

        /// <summary>
        /// 衝突カテゴリー用マスク
        /// </summary>
        ushort CollisionMask { get; set; }

        /// <summary>
        /// 回転を許可するか
        /// </summary>
        bool IsAllowRotation { get; set; }

        /// <summary>
        /// センサーを設定・取得する
        /// </summary>
        IReadOnlyDictionary<string, ISensor> Sensors { get; }

        /// <summary>
        /// OnUpdate時に呼び出される関数のデリゲート
        /// </summary>
        event Action<IEventObject> Update;

        /// <summary>
        /// ダメージを受けるか
        /// </summary>
        bool IsReceiveDamage { get; set; }

        //TODO: Camp

        /// <summary>
        /// 子MapObjectを設定する
        /// </summary>
        /// <param name="name">Object名</param>
        /// <param name="mapObjectPath">MapObjectファイルへのパス</param>
        void SetChild(string name, string mapObjectPath);

        /// <summary>
        /// 子MapObjectを配置する
        /// </summary>
        /// <param name="name">Object名</param>
        /// <param name="position">MapObjectファイルへのパス</param>
        void CreateChild(string name, Vector position);

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
        /// <param name="position">配置する相対座標</param>
        void SetEffect(string name, Vector position);

        /// <summary>
        /// センサーを作成する
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="position">プレイヤーとの相対座標</param>
        /// <param name="diameter">センサーのサイズ</param>
        void SetSensor(string name, Vector position, float diameter = 3);

        /// <summary>
        /// オブジェクトを消去する
        /// </summary>
        void Dispose();

        /// <summary>
        /// イベント時であるか
        /// </summary>
        bool IsEvent { get; }

        /// <summary>
        /// イベント時オブジェクトの入力状態を得る
        /// </summary>
        /// <param name="inputs">入力</param>
        /// <returns>何フレーム押しているか</returns>
        int GetInputState(Inputs inputs);
    }
}
