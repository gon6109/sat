using System;

namespace SatCore.Attribute
{
    /// <summary>
    /// 複数行テキスト入力
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class TextAreaInputAttribute : IOAttribute
    {
        public TextAreaInputAttribute(string itemName)
        {
            ItemName = itemName;
        }

        public string ItemName { get; private set; }
    }
}
