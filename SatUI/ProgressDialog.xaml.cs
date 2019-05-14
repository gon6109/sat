using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
        #region "最大化・最小化・閉じるボタンの非表示設定"

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int GWL_STYLE = -16;
        const int WS_SYSMENU = 0x80000;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(handle, GWL_STYLE);
            style = style & (~WS_SYSMENU);
            SetWindowLong(handle, GWL_STYLE, style);
        }

        #endregion

        INotifyPropertyChanged bindingSource;

        public ProgressDialog(string title, string valuebindingPath, INotifyPropertyChanged bindingSource)
        {
            InitializeComponent();

            Title = title;
            this.bindingSource = bindingSource;

            var bind = new System.Windows.Data.Binding(valuebindingPath);
            bind.Source = bindingSource;
            bind.Mode = System.Windows.Data.BindingMode.OneWay;
            bind.UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged;
            progress.SetBinding(ProgressBar.ValueProperty, bind);

            bindingSource.PropertyChanged += PropertyChanged;
        }

        private void Progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 100)
            {
                Close();
            }
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsCancel" && sender.GetType().GetProperty("IsCancel")?.GetValue(sender) is bool isCancel)
            {
                if (isCancel)
                    Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            bindingSource.PropertyChanged -= PropertyChanged;
        }
    }
}
