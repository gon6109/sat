using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class VectorInputAttribute : IOAttribute
    {
        public VectorInputAttribute(string itemName)
        {
            ItemName = itemName;
        }

        public string ItemName { get; }
    }
}
