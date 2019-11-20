using System;

namespace InspectorModel
{
    /// <summary>
    /// スクリプト
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class ScriptAttribute : BaseAttribute
    {
        public ScriptAttribute(string itemName, string type)
        {
            ItemName = itemName;
            Type = type;
        }

        public string ItemName { get; }
        public string Type { get; }
    }
}
