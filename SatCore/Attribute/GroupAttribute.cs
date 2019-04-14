using System;

namespace SatCore.Attribute
{
    /// <summary>
    /// グループ化
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class GroupAttribute : IOAttribute
    {
        // This is a positional argument
        public GroupAttribute(string itemName)
        {
            ItemName = itemName;
        }

        public string ItemName { get; private set; }
    }
}
