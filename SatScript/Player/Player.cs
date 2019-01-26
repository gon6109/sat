using AltseedScript.Common;
using SatScript.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SatScript.Player
{
    public class Player
    {
        /// <summary>
        /// 使用されているキャラクター名
        /// </summary>
        public static IEnumerable<string> Players
           => (asd.Engine.CurrentScene as SatPlayer.Game)?
           .CanUsePlayers
           .Select(obj => obj.Name);

        public static Player CurrentPlayer
            => ToScript((asd.Engine.CurrentScene as SatPlayer.Game)?
            .Layers.OfType<SatPlayer.MainMapLayer2D>()
            .FirstOrDefault()?
            .Player);

        static Player ToScript(IPlayer player)
        {
            return new Player()
            {
                _playerImp = player,
            };
        }

        IPlayer _playerImp;

        /// <summary>
        /// キャラクター名
        /// </summary>
        public string Name => _playerImp.Name;

        /// <summary>
        /// 座標
        /// </summary>
        public Vector Position => _playerImp.Position;

        /// <summary>
        /// HP初期値100
        /// </summary>
        public int HP => _playerImp.HP;

        /// <summary>
        /// アニメーション状態
        /// </summary>
        public string State => _playerImp.State;

        public Color Color => _playerImp.Color;

        /// <summary>
        /// 衝突情報
        /// </summary>
        public ICollision Collision => _playerImp.Collision;

        /// <summary>
        /// 地面と接しているか
        /// </summary>
        bool IsCollidedWithGround => _playerImp.IsCollidedWithGround;

        /// <summary>
        /// 速度
        /// </summary>
        Vector Velocity => _playerImp.Velocity;

        /// <summary>
        /// イベント時か
        /// </summary>
        bool IsEvent => _playerImp.IsEvent;
    }
}
