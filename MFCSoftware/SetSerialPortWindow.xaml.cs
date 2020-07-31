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
using CommonLib.Mvvm;
using MFCSoftware.ViewModels;

namespace MFCSoftware
{
    /// <summary>
    /// SetSerialPortWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetSerialPortWindow : Window
    {
        private SetSerialPortWindowViewModel viewModel;
        public SetSerialPortWindow()
        {
            InitializeComponent();
            viewModel = new SetSerialPortWindowViewModel();
            this.DataContext = viewModel;
        }

        private void SetSerialPort(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(viewModel.PortName)) return;
            if (viewModel.BaudRate == 0) return;
            try
            {
                viewModel.SetSerialPort();
                this.Close();
            }
            catch
            {
                MessageBox.Show("串口连接失败，请检查线缆连接情况或重新插拔USB并重启软件。");
            }
        }
    }
}
