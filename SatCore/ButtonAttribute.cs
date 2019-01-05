using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    /// <summary>
    /// ボタン
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class ButtonAttribute : IOAttribute
    {
        public string Name { get; private set; }

        public ButtonAttribute(string name)
        {
            Name = name;
        }
    }
}
