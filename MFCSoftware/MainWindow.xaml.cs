using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MFCSoftware
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenSetSerialPortWindow(object sender, RoutedEventArgs e)
        {
            SetSerialPortWindow window = new SetSerialPortWindow();
            window.Owner = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }

        private void OpenSetAddressWindow(object sender, RoutedEventArgs e)
        {
            SetAddressWindow window = new SetAddressWindow();
            window.Owner = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }

        //private MFCSoftwareWindow mfcWindow;
        private void OpenMFCSoftwareWindow(object sender, RoutedEventArgs e)
        {
            var mfcWindow = new MFCSoftwareWindow();
            mfcWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mfcWindow.Show();
        }
    }
}
