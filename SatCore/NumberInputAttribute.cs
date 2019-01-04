using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class NumberInputAttribute : IOAttribute
    {
        public string ItemName { get; set; }

        public NumberInputAttribute(string itemName)
        {
            ItemName = itemName;
        }
    }
}
