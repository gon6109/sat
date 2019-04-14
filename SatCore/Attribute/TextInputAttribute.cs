using System;

namespace SatCore.Attribute
{
    /// <summary>
    /// テキスト入力
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class TextInputAttribute : IOAttribute
    {
        public TextInputAttribute(string itemName, bool isPropertyChanged = true)
        {
            ItemName = itemName;
            IsPropertyChanged = isPropertyChanged;
        }

        public bool IsPropertyChanged { get; private set; }
        public string ItemName { get; private set; }
    }
}
