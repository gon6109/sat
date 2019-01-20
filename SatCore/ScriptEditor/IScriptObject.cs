using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SatPlayer;

namespace SatCore.ScriptEditor
{
    public interface IScriptObject
    {
        string ScriptOptionName { get; }
        string Code { get; set; }
        bool IsSuccessBuild { get; }

        bool IsSingle { get; }
        bool IsPreparePlayer { get; }

        void Run();
        object Clone();
    }
}
