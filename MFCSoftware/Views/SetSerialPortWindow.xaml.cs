using System.Windows;
using MFCSoftware.ViewModels;

namespace MFCSoftware.Views
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
