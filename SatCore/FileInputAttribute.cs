using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class FileInputAttribute : IOAttribute
    {
        public FileInputAttribute(string itemName, string filter = "All File|*.*", bool isAutoConvertRelativePath = true)
        {
            ItemName = itemName;
            Filter = filter;
            IsAutoConvertRelativePath = isAutoConvertRelativePath;
        }

        public bool IsAutoConvertRelativePath { get; }

        public string ItemName { get; }

        public string Filter { get; }
    }
}
