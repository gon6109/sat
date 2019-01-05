using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    /// <summary>
    /// 座標入力
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class VectorInputAttribute : IOAttribute
    {
        public VectorInputAttribute(string itemName)
        {
            ItemName = itemName;
        }

        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; }
    }
}
