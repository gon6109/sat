using System;

namespace SatCore.Attribute
{
    /// <summary>
    /// テキスト表示プロパティ
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class TextOutputAttribute : IOAttribute
    {
        public string ItemName { get; private set; }

        public TextOutputAttribute(string itemName)
        {
            ItemName = itemName;
        }
    }
}
