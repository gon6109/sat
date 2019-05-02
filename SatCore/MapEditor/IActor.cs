using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor
{
    /// <summary>
    /// Event用インターフェース
    /// </summary>
    public interface IActor
    {
        string Path { get; }
        int ID { get; }
        bool IsEvent { get; set; }
        asd.Vector2DF Position { get; set; }

        Queue<Dictionary<BaseComponent.Inputs, bool>> MoveCommands { get; }
        PhysicAltseed.PhysicalShape CollisionShape { get; }
        asd.RectangleShape GroundCollision { get; }
        asd.Texture2D Texture { get; set; }
        asd.Layer2D Layer { get; }

        void SetCollision(MapLayer mapLayer);

        void OnUpdate();
    }
}
