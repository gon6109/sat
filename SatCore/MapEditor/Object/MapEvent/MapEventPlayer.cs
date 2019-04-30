using BaseComponent;
using SatPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor.Object.MapEvent
{
    /// <summary>
    /// MapEvent用Player
    /// </summary>
    public class MapEventPlayer : SatPlayer.Game.Object.Player
    {
        protected override void OnAdded()
        {
            if (Layer is MapLayer map)
                CollisionShape = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Dynamic, map.PhysicalWorld);
            base.OnAdded();
        }

        public static async new Task<MapEventPlayer> CreatePlayerAsync(string playerDataPath, int playerGroup = 0)
        {
            try
            {
                var player = new MapEventPlayer();
                player.PlayerGroup = playerGroup;
                player.Path = playerDataPath;
                var stream = await IO.GetStreamAsync(playerDataPath);
                using (stream)
                {
                    var script = ScriptOption.ScriptOptions["Player"].CreateScript<object>(Encoding.UTF8.GetString(stream.ToArray()));
                    await script.RunAsync(player);
                }
                if (player.Texture == null)
                    player.State = player.AnimationPart.FirstOrDefault().Key;
                return player;
            }
            catch
            {
                throw;
            }
        }
    }
}
