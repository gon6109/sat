using SatCore.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SatUI
{
    /// <summary>
    /// Property.xaml の相互作用ロジック
    /// </summary>
    public partial class Property : UserControl
    {
        public Property(string name, object bindingSource)
        {
            InitializeComponent();

            expander.Header = name;
            foreach (MemberInfo info in bindingSource.GetType().GetMembers(
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly))
            {
                CreateControl(info, bindingSource);
            }
        }

        public Property(string name, object[] bindingSources)
        {
            InitializeComponent();

            expander.Header = name;
            foreach (var item in bindingSources)
            {
                foreach (MemberInfo info in item.GetType().GetMembers(
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly))
                {
                    CreateControl(info, item);
                }
            }
        }

        void CreateControl(MemberInfo info, object bindingSource)
        {
            if (info is PropertyInfo) CreatePropertyControl((PropertyInfo)info, bindingSource);
            else if (info is MethodInfo) CreateMethodControl((MethodInfo)info, bindingSource);
        }

        void CreatePropertyControl(PropertyInfo info, object bindingSource)
        {
            Attribute[] controls = Attribute.GetCustomAttributes(info, typeof(IOAttribute));
            foreach (Attribute att in controls)
            {
                TextOutputAttribute textOutput = att as TextOutputAttribute;
                if (textOutput != null)
                {
                    PropertyItems.Children.Add(new TextOutput(textOutput.ItemName, info.GetValue(bindingSource).ToString()));
                    return;
                }

                DirectoryInputAttribute directoryInput = att as DirectoryInputAttribute;
                if (directoryInput != null && info.PropertyType == typeof(string))
                {
                    PropertyItems.Children.Add(new DirectoryInput(directoryInput.ItemName, info.Name, bindingSource, directoryInput.IsAutoConvertRelativePath, MapEditor.PlayerExePathGetter));
                    return;
                }

                FileInputAttribute fileInput = att as FileInputAttribute;
                if (fileInput != null && info.PropertyType == typeof(string))
                {
                    PropertyItems.Children.Add(new FileInput(fileInput.ItemName, info.Name, bindingSource, fileInput.Filter, fileInput.IsAutoConvertRelativePath, MapEditor.PlayerExePathGetter));
                    return;
                }

                VectorInputAttribute vectorInput = att as VectorInputAttribute;
                if (vectorInput != null && info.PropertyType == typeof(asd.Vector2DF))
                {
                    PropertyItems.Children.Add(new VectorInput(vectorInput.ItemName, info.Name, bindingSource));
                    return;
                }

                BoolInputAttribute boolInput = att as BoolInputAttribute;
                if (boolInput != null && info.PropertyType == typeof(bool))
                {
                    PropertyItems.Children.Add(new BoolInput(boolInput.ItemName, info.Name, bindingSource));
                    return;
                }

                NumberInputAttribute numberInput = att as NumberInputAttribute;
                if (numberInput != null && info.PropertyType == typeof(int))
                {
                    PropertyItems.Children.Add(new NumberInput(numberInput.ItemName, info.Name, bindingSource));
                    return;
                }

                TextInputAttribute textInput = att as TextInputAttribute;
                if (textInput != null)
                {
                    PropertyItems.Children.Add(new TextInput(textInput.ItemName, info.Name, bindingSource, textInput.IsPropertyChanged));
                    return;
                }

                TextAreaInputAttribute textAreaInput = att as TextAreaInputAttribute;
                if (textAreaInput != null)
                {
                    PropertyItems.Children.Add(new TextAreaInput(textAreaInput.ItemName, info.Name, bindingSource));
                    return;
                }

                GroupAttribute groupAttribute = att as GroupAttribute;
                if (groupAttribute != null)
                {
                    PropertyItems.Children.Add(new Property(groupAttribute.ItemName, info.GetValue(bindingSource)));
                    return;
                }

                ListInputAttribute listInput = att as ListInputAttribute;
                if (listInput != null && info.PropertyType.GetGenericArguments()[0] != null
                    && typeof(IListInput).IsAssignableFrom(info.PropertyType.GetGenericArguments()[0]))
                {
                    PropertyItems.Children.Add(
                        new ListInput(listInput.GroupName, info.GetValue(bindingSource), bindingSource,
                        listInput.SelectedObjectBindingPath, listInput.AdditionButtonEventMethodName, listInput.IsVisibleRemoveButtton));
                    return;
                }
            }
        }

        void CreateMethodControl(MethodInfo info, object bindingSource)
        {
            Attribute[] controls = Attribute.GetCustomAttributes(info, typeof(IOAttribute));
            foreach (Attribute att in controls)
            {
                ButtonAttribute buttonAttribute = att as ButtonAttribute;
                if (buttonAttribute != null)
                {
                    var temp = new Button();
                    temp.Padding = new Thickness(4, 2, 4, 2);
                    temp.HorizontalAlignment = HorizontalAlignment.Stretch;
                    temp.VerticalAlignment = VerticalAlignment.Center;
                    temp.Content = buttonAttribute.Name;
                    temp.Click += (object sender, RoutedEventArgs e) =>
                    {
                        info.Invoke(bindingSource, null);
                    };
                    PropertyItems.Children.Add(temp);
                    return;
                }
            }
        }
    }
}
