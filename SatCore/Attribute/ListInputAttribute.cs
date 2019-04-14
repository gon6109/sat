using System;

namespace SatCore.Attribute
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class ListInputAttribute : IOAttribute
    {
        public ListInputAttribute(string groupName, string selectedObjectBindingPath = "", string additionButtonEventMethodName = "", bool isVisibleRemoveButtton = true)
        {
            GroupName = groupName;
            AdditionButtonEventMethodName = additionButtonEventMethodName;
            SelectedObjectBindingPath = selectedObjectBindingPath;
            IsVisibleRemoveButtton = isVisibleRemoveButtton;
        }

        public string GroupName { get; set; }
        public string AdditionButtonEventMethodName { get; set; }
        public string SelectedObjectBindingPath { get; set; }
        public bool IsVisibleRemoveButtton { get; set; }
    }

    public interface IListInput
    {
        string Name { get; }
    }
}
