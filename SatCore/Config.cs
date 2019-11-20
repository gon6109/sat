using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    [Serializable]
    public class Config
    {
        public static Config Instance { get; set; } = new Config();

        public string RootPath;

        public bool isFullSize;

        public int Width;

        public int Height;

        public string ScriptPlayerPath;
    }
}
