using System;

namespace SatCore.Attribute
{
    /// <summary>
    /// 座標入力
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
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
