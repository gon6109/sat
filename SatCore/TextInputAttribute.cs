using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class TextInputAttribute : IOAttribute
    {
        // This is a positional argument
        public TextInputAttribute(string itemName, bool isPropertyChanged = true)
        {
            ItemName = itemName;
            IsPropertyChanged = isPropertyChanged;
        }

        public bool IsPropertyChanged { get; private set; }
        public string ItemName { get; private set; }
    }
}
