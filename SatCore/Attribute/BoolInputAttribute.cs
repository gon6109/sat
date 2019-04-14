using System;

namespace SatCore.Attribute
{
    /// <summary>
    /// 真偽値入力
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class BoolInputAttribute : IOAttribute
    {
        public string ItemName { get; set; }

        public BoolInputAttribute(string itemName)
        {
            ItemName = itemName;
        }
    }
}
