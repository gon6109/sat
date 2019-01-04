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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SatUI
{
    /// <summary>
    /// DirectoryInput.xaml の相互作用ロジック
    /// </summary>
    public partial class DirectoryInput : UserControl
    {
        public bool IsAutoConvertRelativePath { get; private set; }
        public string RootPath { get; private set; }

        public DirectoryInput(string itemName, string bindingPath, object bindingSource, bool isAutoConvertRelativePath = true, string rootPath = "")
        {
            InitializeComponent();

            var bind = new Binding(bindingPath);
            bind.Source = bindingSource;
            bind.Mode = BindingMode.TwoWay;
            bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Path.SetBinding(TextBox.TextProperty, bind);

            ItemName.Content = itemName;
            RootPath = rootPath;
            IsAutoConvertRelativePath = isAutoConvertRelativePath;
        }

        private void Dialog_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog openFileDialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            openFileDialog.InitialDirectory = "";
            openFileDialog.IsFolderPicker = true;
            openFileDialog.RestoreDirectory = true;
            var result = openFileDialog.ShowDialog(new WindowInteropHelper(Program.MainWindow).Handle);
            if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok) Path.Text = openFileDialog.FileName + "\\";
            if (IsAutoConvertRelativePath) Path.Text = SatCore.Path.GetRelativePath(Path.Text, RootPath);
        }
    }
}
