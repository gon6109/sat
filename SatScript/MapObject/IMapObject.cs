using AltseedScript.Common;
using SatScript.Collision;
using SatScript.Damage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatScript.MapObject
{
    /// <summary>
    /// MapObjectスクリプト用インターフェース
    /// </summary>
    public interface IMapObject
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
        /// 現在のアニメーションを最初のフレームにする
        /// </summary>
        void ResetCurrentStateAnimation();

        /// <summary>
        /// MapObjectのタイプ
        /// </summary>
        MapObjectType MapObjectType { get; set; }

        /// <summary>
        /// 衝突情報
        /// </summary>
        ICollision Collision { get; }

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
        event Action<IMapObject> Update;

        /// <summary>
        /// ダメージを受けるか
        /// </summary>
        bool IsReceiveDamage { get; set; }

        /// <summary>
        /// 陣営
        /// </summary>
        int DamageGroup { get; set; }

        /// <summary>
        /// ダメージ情報
        /// </summary>
        IDamage Damage { get; }

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
        /// 攻撃
        /// </summary>
        /// <param name="position">攻撃範囲左上相対座標</param>
        /// <param name="size">攻撃範囲サイズ</param>
        /// <param name="damage">ダメージ量</param>
        /// <param name="frame">フレーム数</param>
        /// <param name="isSastainable">持続するか</param>
        /// <param name="knockBack">ノックバック率</param>
        /// <param name="takeDown">ダウン時間</param>
        /// <param name="priority">優先度</param>
        void Attack(Vector position, Vector size, int damage, int frame, bool isSastainable = false, int knockBack = 0, int takeDown = 0, int priority = 0);

        /// <summary>
        /// 直接攻撃
        /// </summary>
        /// <param name="position">攻撃範囲左上相対座標</param>
        /// <param name="size">攻撃範囲サイズ</param>
        /// <param name="to">相手</param>
        /// <param name="damage">ダメージ量</param>
        /// <param name="frame">フレーム数</param>
        /// <param name="isSastainable">持続するか</param>
        /// <param name="knockBack">ノックバック率</param>
        /// <param name="takeDown">ダウン時間</param>
        /// <param name="priority">優先度</param>
        void DirectAttackToMapObject(Vector position, Vector size, MapObject to, int damage, int frame, bool isSastainable = false, int knockBack = 0, int takeDown = 0, int priority = 0);

        /// <summary>
        /// プレイヤー直接攻撃
        /// </summary>
        /// <param name="position">攻撃範囲左上相対座標</param>
        /// <param name="size">攻撃範囲サイズ</param>
        /// <param name="damage">ダメージ量</param>
        /// <param name="frame">フレーム数</param>
        /// <param name="isSastainable">持続するか</param>
        /// <param name="knockBack">ノックバック率</param>
        /// <param name="takeDown">ダウン時間</param>
        /// <param name="priority">優先度</param>
        void DirectAttackToPlayer(Vector position, Vector size, int damage, int frame, bool isSastainable = false, int knockBack = 0, int takeDown = 0, int priority = 0);

        /// <summary>
        /// オブジェクトを消去する
        /// </summary>
        void Dispose();
    }

    public interface ISensor
    {
        Vector Position { get; set; }
        float Radius { get; set; }
        ICollision Collision { get; }
    }

    /// <summary>
    /// マップオブジェクトのタイプ
    /// </summary>
    public enum MapObjectType
    {
        /// <summary>
        /// 物理対応
        /// </summary>
        Active,
        /// <summary>
        /// 物理非対応
        /// </summary>
        Passive,
    }
}
