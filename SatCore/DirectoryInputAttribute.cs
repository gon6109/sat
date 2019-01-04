using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class DirectoryInputAttribute : IOAttribute
    {
        public DirectoryInputAttribute(string itemName, bool isAutoConvertRelativePath = true)
        {
            ItemName = itemName;
            IsAutoConvertRelativePath = isAutoConvertRelativePath;
        }

        public bool IsAutoConvertRelativePath { get; }

        public string ItemName { get; }
    }
}
