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
    /// NumberInput.xaml の相互作用ロジック
    /// </summary>
    public partial class NumberInput : UserControl
    {
        public NumberInput(string itemName, string bindingPath, object bindingSource)
        {
            InitializeComponent();
            ItemName.Content = itemName;
            var bind = new System.Windows.Data.Binding(bindingPath);
            bind.Source = bindingSource;
            bind.Mode = System.Windows.Data.BindingMode.TwoWay;
            bind.UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged;
            Number.SetBinding(TextBox.TextProperty, bind);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Number.Text = (Convert.ToInt32(Number.Text) + 1).ToString();
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            Number.Text = (Convert.ToInt32(Number.Text) - 1).ToString();
        }
    }
}
