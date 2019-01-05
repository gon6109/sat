using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    /// <summary>
    /// グループ化
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class GroupAttribute : IOAttribute
    {
        // This is a positional argument
        public GroupAttribute(string itemName)
        {
            ItemName = itemName;
        }

        public string ItemName { get; private set; }
    }
}
