using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.ScriptEditor
{
    public class MapLayer : SatPlayer.Game.MapLayer
    {
        public bool IsPreparePlayer { get; set; }

        public new EditablePlayer Player
        {
            get => base.Player as EditablePlayer;
            protected set => base.Player = value;
        }

        public MapLayer()
        {
            PhysicalWorld = new PhysicAltseed.PhysicalWorld(new asd.RectF(new asd.Vector2DF(), OriginDisplaySize), new asd.Vector2DF(0, 8000));
        }

        protected override void OnAdded()
        {
            if (IsPreparePlayer)
            {
                Player = new EditablePlayer();
                AddObject(Player);
            }
            if (IsPreparePlayer && Scene is ScriptEditor scene && scene.PlayerPath != null)
            {
                using (var stream = IO.GetStream(scene.PlayerPath))
                {
                    Player.Code = Encoding.UTF8.GetString(stream.ToArray());
                }
                _ = Player.Run();
                Player.Position = OriginDisplaySize / 2;
            }
        }

        protected override void OnUpdated()
        {
            base.OnUpdated();
        }
    }
}
