using SatPlayer.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game
{
    public interface IDamageManeger
    {
        List<DamageRect> Damages { get; }
    }
}
