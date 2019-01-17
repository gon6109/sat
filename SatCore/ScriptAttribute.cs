using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    /// <summary>
    /// スクリプト
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class ScriptAttribute : IOAttribute
    {
        public ScriptAttribute(string itemName, string type)
        {
            ItemName = itemName;
            Type = type;
        }

        public string ItemName { get; }
        public string Type { get; }
    }
}
