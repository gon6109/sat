using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    public interface IEffectManeger
    {
        Dictionary<string, Effect> Effects { get; }

        void LoadEffect(string animationGroup, string extension, int sheets, string name, int interval);
        void SetEffect(string name, asd.Vector2DF position);
    }
}
