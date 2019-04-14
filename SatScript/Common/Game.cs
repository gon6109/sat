using AltseedScript.Common;
using SatPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatScript.Common
{
    /// <summary>
    /// ゲームシーン
    /// </summary>
    public static class Game
    {
        /// <summary>
        /// マップ遷移
        /// </summary>
        /// <param name="dstMapPath">遷移先マップへのパス</param>
        /// <param name="vector">遷移先座標</param>
        /// <param name="doorID">遷移先ドア</param>
        public static void MoveMap(string dstMapPath, Vector vector, int? doorID = null)
        {
            var scene = asd.Engine.CurrentScene as SatPlayer.Game.GameScene;
            scene?.ChangeMap
                (dstMapPath,
                scene.CanUsePlayers,
                vector.ToAsdVector(),
                doorID ?? -1);
        }

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        public static void GameOver()
        {
            var scene = asd.Engine.CurrentScene as SatPlayer.Game.GameScene;
            scene?.GameOver();
        }

        /// <summary>
        /// マップ
        /// </summary>
        public static class Map
        {
            /// <summary>
            /// マップ名
            /// </summary>
            public static string Name => (asd.Engine.CurrentScene as SatPlayer.Game.GameScene)?.MapName;
        }
    }
}
