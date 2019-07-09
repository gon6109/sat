using BaseComponent;
using PhysicAltseed;
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
    public class MapEventPlayer : SatPlayer.Game.Object.Player, IActor
    {
        PhysicalShape IActor.CollisionShape => CollisionShape;

        protected override void OnAdded()
        {
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
                    foreach (var item in player.LoadTextureTasks)
                    {
                        await player.AddAnimationPartAsync(item.animationGroup, item.extension, item.sheets, item.partName, item.interval);
                    }
                    player.State = player.State;
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

        void IActor.SetCollision(MapLayer mapLayer)
        {
            CollisionShape = new PhysicalRectangleShape(PhysicalShapeType.Dynamic, mapLayer.PhysicalWorld);

            CollisionShape.Density = 2.5f;
            CollisionShape.Restitution = 0.0f;
            CollisionShape.Friction = 0.0f;
            CollisionShape.GroupIndex = -1;
            DrawingPriority = 2;

            SetCollision();
        }

        protected override void OnUpdate()
        {
            if (Layer is MapLayer map)
                IsCollidedWithGround = map.Obstacles.Any(obj => obj.GetIsCollidedWith(GroundCollision));
            base.OnUpdate();
        }

        void IActor.OnUpdate()
        {
            OnUpdate();
        }
    }
}
