using System;

namespace SatCore.Attribute
{
    /// <summary>
    /// ファイル入力
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class FileInputAttribute : IOAttribute
    {
        public FileInputAttribute(string itemName, string filter = "All File|*.*", bool isAutoConvertRelativePath = true)
        {
            ItemName = itemName;
            Filter = filter;
            IsAutoConvertRelativePath = isAutoConvertRelativePath;
        }

        public bool IsAutoConvertRelativePath { get; }

        public string ItemName { get; }

        public string Filter { get; }
    }
}
