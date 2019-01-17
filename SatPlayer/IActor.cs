using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using asd;

namespace SatPlayer
{
    /// <summary>
    /// Event用インターフェース
    /// </summary>
    public interface IActor
    {
        bool IsUseName { get; }
        string Name { get; }
        int ID { get; }
        bool IsEvent { get; set; }
        Vector2DF Position { get; set; }

        Queue<Dictionary<BaseComponent.Inputs, bool>> MoveCommands { get; }
        PhysicAltseed.PhysicalShape CollisionShape { get; }
        asd.RectangleShape GroundShape { get; }
        Texture2D Texture { get; set; }
        Layer2D Layer { get; }

        void OnUpdate();
    }
}
