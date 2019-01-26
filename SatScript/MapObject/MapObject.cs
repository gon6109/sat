using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AltseedScript.Common;
using SatPlayer;
using SatScript.Collision;

namespace SatScript.MapObject
{
    /// <summary>
    /// MapObjectに関する制御を行う。
    /// </summary>
    public class MapObject
    {
        public static IEnumerable<MapObject> MapObjects
            => (asd.Engine.CurrentScene as SatPlayer.Game)?
            .Layers.OfType<SatPlayer.MainMapLayer2D>()
            .FirstOrDefault()?
            .Objects.OfType<IMapObject>()
            .Select(obj => ToScript(obj));

        static MapObject ToScript(IMapObject mapObject)
        {
            return new MapObject()
            {
                _mapObjectImp = mapObject,
            };
        }

        IMapObject _mapObjectImp;

        /// <summary>
        /// オブジェクトにつけられたユーザー定義のタグ
        /// </summary>
        public string Tag => _mapObjectImp.Tag;

        /// <summary>
        /// 現在座標
        /// </summary>
        public Vector Position => _mapObjectImp.Position;

        /// <summary>
        /// HP
        /// </summary>
        public int HP => _mapObjectImp.HP;

        /// <summary>
        /// アニメーション状態
        /// </summary>
        public string State => _mapObjectImp.State;

        /// <summary>
        /// MapObjectのタイプ
        /// </summary>
        public MapObjectType MapObjectType => _mapObjectImp.MapObjectType;

        /// <summary>
        /// 衝突情報
        /// </summary>
        public ICollision Collision => _mapObjectImp.Collision;

        /// <summary>
        /// 速度
        /// </summary>
        public Vector Velocity => _mapObjectImp.Velocity;

        /// <summary>
        /// 回転を許可するか
        /// </summary>
        public bool IsAllowRotation => _mapObjectImp.IsAllowRotation;

        /// <summary>
        /// ダメージを受けるか
        /// </summary>
        public bool IsRecieveDamage => _mapObjectImp.IsReceiveDamage;

        //TODO: Camp

        /// <summary>
        /// オブジェクトを消去する
        /// </summary>
        public void Dispose()
        {
            _mapObjectImp.Dispose();
        }
    }
}
