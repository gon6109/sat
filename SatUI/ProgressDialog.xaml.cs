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
using System.Windows.Shapes;

namespace SatUI
{
    /// <summary>
    /// ProgressDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressDialog : Window
    {
        public ProgressDialog(string title, string bindingPath, object bindingSource)
        {
            InitializeComponent();

            Title = title;

            var bind = new System.Windows.Data.Binding(bindingPath);
            bind.Source = bindingSource;
            bind.Mode = System.Windows.Data.BindingMode.TwoWay;
            bind.UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged;
            progress.SetBinding(ProgressBar.ValueProperty, bind);
        }
    }
}
