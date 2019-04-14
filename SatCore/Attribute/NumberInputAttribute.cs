using System;

namespace SatCore.Attribute
{
    /// <summary>
    /// 整数入力
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class NumberInputAttribute : IOAttribute
    {
        public string ItemName { get; set; }

        public NumberInputAttribute(string itemName)
        {
            ItemName = itemName;
        }
    }
}
