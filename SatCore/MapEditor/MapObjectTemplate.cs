using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor
{
    public class MapObjectTemplate : IListInput
    {
        [TextInput("名前")]
        public string Name { get; set; }

        [FileInput("スクリプト", "Script File|*.csx|All File|*.*")]
        public string ScriptPath { get; set; }
        
        [FileInput("モーション", "Motion File|*.mo|All File|*.*")]
        public string MotionPath { get; set; }

        public MapObjectTemplate()
        {
            Name = "";
            ScriptPath = "";
            MotionPath = "";
        }
    }
}
