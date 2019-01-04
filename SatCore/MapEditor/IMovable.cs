using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor
{
    public interface IMovable
    {
        void StartMove();
        void EndMove();
    }
}
