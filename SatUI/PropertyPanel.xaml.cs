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
    /// PropertyPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class PropertyPanel : UserControl
    {
        public PropertyPanel()
        {
            InitializeComponent();
        }

        public void AddProperty(Property property)
        {
            Properties.Children.Add(property);
            Focus();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ResetProperty(ResetMode mode)
        {
            switch (mode)
            {
                case ResetMode.Map:
                    RemoveProperty(obj => (string)((Property)obj).expander.Header != "Map" && 
                        (string)((Property)obj).expander.Header != "General");
                    break;
                case ResetMode.General:
                    RemoveProperty(obj => (string)((Property)obj).expander.Header != "General");
                    break;
                default:
                    break;
            }
            Focus();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        void RemoveProperty(Func<UIElement, bool> func)
        {
            while(Properties.Children.Cast<UIElement>().Any(obj => (obj is Property) && func(obj)))
            {
                Properties.Children.Remove(Properties.Children.Cast<UIElement>().Where(obj => (obj is Property) && func(obj)).First());
            }
        }

        public enum ResetMode
        {
            Map,
            General,
        }
    }
}
