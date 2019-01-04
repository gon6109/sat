using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    public interface IDamageManeger
    {
        List<DamageRect> Damages { get; }
    }
}
