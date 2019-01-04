using System;
using System.Collections.Generic;
using System.Linq;
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
    /// BoolInput.xaml の相互作用ロジック
    /// </summary>
    public partial class BoolInput : UserControl
    {
        public BoolInput(string itemName, string bindingPath, object bindingSource)
        {
            InitializeComponent();
            checkBox.Content = itemName;
            var bind = new System.Windows.Data.Binding(bindingPath);
            bind.Source = bindingSource;
            bind.Mode = System.Windows.Data.BindingMode.TwoWay;
            bind.UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged;
            checkBox.SetBinding(CheckBox.IsCheckedProperty, bind);
        }
    }
}
