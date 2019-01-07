using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    /// <summary>
    /// スクリプト用インターフェース
    /// </summary>
    public interface IMapObjectData
    {
        asd.Vector2DF Position { get; }
        string GroupName { get; }
        MapObjectType MapObjectType { get; }
        MainMapLayer2D RefMainMapLayer2D { get; }
        MapObject.Sensor GetSensorData(string name);
        string State { get; }
        bool GetIsColligedWith(asd.Shape shape);
        PhysicAltseed.PhysicalShape CollisionShape { get; }
    }
}
