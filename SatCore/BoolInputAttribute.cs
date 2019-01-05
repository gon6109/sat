using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    /// <summary>
    /// 真偽値入力
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class BoolInputAttribute : IOAttribute
    {
        public string ItemName { get; set; }

        public BoolInputAttribute(string itemName)
        {
            ItemName = itemName;
        }
    }
}
