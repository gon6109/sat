using SatCore.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor
{
    /// <summary>
    /// マップオブジェクトテンプレート
    /// </summary>
    public class MapObjectTemplate : IListInput
    {
        [TextInput("名前")]
        public string Name { get; set; }

        [FileInput("スクリプト", "Object File|*.mobj;*.eobj|All File|*.*")]
        public string ScriptPath { get; set; }

        public MapObjectTemplate()
        {
            Name = "";
            ScriptPath = "";
        }
    }
}
