using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.ScriptEditor
{
    public class MainMapLayer2D : SatPlayer.MainMapLayer2D
    {
        public bool IsPreparePlayer { get; set; }

        public MainMapLayer2D()
        {
            PhysicalWorld = new PhysicAltseed.PhysicalWorld(new asd.RectF(new asd.Vector2DF(), Base.ScreenSize.To2DF()), new asd.Vector2DF(0, 2000));
        }

        protected override void OnAdded()
        {
            //TODO: デフォルトリソース
            //if (IsPreparePlayer)
            //{
            //    Player = new SatPlayer.Player("Player/yuki.pd");
            //    Player.Position = Base.ScreenSize.To2DF() / 2;
            //    Player.CollisionShape = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Dynamic, PhysicalWorld);
            //    AddObject(Player);
            //}
        }

        protected override void OnUpdating()
        {

        }

        protected override void OnUpdated()
        {

        }
    }
}
