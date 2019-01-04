using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
    /// PlayersListDialogUI.xaml の相互作用ロジック
    /// </summary>
    public partial class PlayersListDialogUI : Window
    {
        public SatCore.PlayersListDialog PlayersListDialog { get; private set; }
        public SatCore.PlayersListDialogResult Result { get; private set; }

        public PlayersListDialogUI(SatCore.PlayersListDialog playersListDialog)
        {
            InitializeComponent();

            DataContext = playersListDialog.PlayerNames;
            PlayersListDialog = playersListDialog;
            if (listBox.SelectedItem == null) button1.IsEnabled = false;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Result = SatCore.PlayersListDialogResult.OK;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Result = SatCore.PlayersListDialogResult.Cancel;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayersListDialog.PlayerName = e.AddedItems[0] as string;
            if (listBox.SelectedItem != null) button1.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Result = SatCore.PlayersListDialogResult.Close;
        }
    }
}
