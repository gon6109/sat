using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    /// <summary>
    /// テキスト表示プロパティ
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class TextOutputAttribute : IOAttribute
    {
        public string ItemName { get; private set; }

        public TextOutputAttribute(string itemName)
        {
            ItemName = itemName;
        }
    }
}
