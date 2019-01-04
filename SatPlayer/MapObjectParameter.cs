using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    public class MapObjectParameter<T>
    {
        Dictionary<MapObject, T> datas;

        public MapObjectParameter()
        {
            datas = new Dictionary<MapObject, T>();    
        }

        public T this [MapObject obj]
        {
            get
            {
                if (!datas.ContainsKey(obj)) return default(T);
                return datas[obj];
            }
            set => datas[obj] = value;
        }
    }
}
