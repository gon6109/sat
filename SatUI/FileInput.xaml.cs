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
    /// FileInput.xaml の相互作用ロジック
    /// </summary>
    public partial class FileInput : UserControl
    {
        public string Filter { get; private set; }
        public bool IsAutoConvertRelativePath { get; private set; }
        public string RootPath { get; private set; }

        public FileInput(string itemName, string bindingPath, object bindingSource, string filter = "All File|*.*",
            bool isAutoConvertRelativePath = true, string rootPath = "")
        {
            InitializeComponent();

            var bind = new System.Windows.Data.Binding(bindingPath);
            bind.Source = bindingSource;
            bind.Mode = System.Windows.Data.BindingMode.TwoWay;
            bind.UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged;
            Path.SetBinding(TextBox.TextProperty, bind);

            ItemName.Content = itemName;
            Filter = filter;
            RootPath = rootPath;
            IsAutoConvertRelativePath = isAutoConvertRelativePath;
        }

        private void Dialog_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.FileName = "";
            openFileDialog.Filter = Filter;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    if (IsAutoConvertRelativePath)
                        Path.Text = SatCore.Path.GetRelativePath(Path.Text, RootPath);
                    else
                        Path.Text = openFileDialog.FileName;

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
