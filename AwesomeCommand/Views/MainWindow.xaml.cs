using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows;
using AwesomeCommand.UIModels;
using AwesomeCommand.ViewModels;

namespace AwesomeCommand.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SerialPortInstance _instance;
        public MainWindow()
        {
            InitializeComponent();
            _instance = (DataContext as MainViewModel).Instance;
            _instance.SerialPort.DataReceived += SerialPort_DataReceived;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = _instance.SerialPort;
            int count = port.BytesToWrite;
            byte[] datas = new byte[count];
            port.Read(datas, 0, count);
            string chars = Encoding.Default.GetString(datas);
            ResultTextBox.AppendText(chars);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./autoui.xml");
            if (!File.Exists(file))
            {
                _ = MessageBox.Show("丢失文件autoui.xml！");
                return;
            }
            AutoUI autoUI = AutoUI.GetUIAuto(file);
            autoUI.Executed += AutoUI_Executed;
            _ = AutoUIGrid.Children.Add(autoUI.Build());
        }

        private void AutoUI_Executed(IAutoBuilder build)
        {
            if (build is UIAction action)
            {
                (bool success, string message) = action.TryParse(out string command);
                if (!success)
                {
                    _ = MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (action.IsAuthorized)
                {
                    ConfirmPasswordDialog dialog = new ConfirmPasswordDialog
                    {
                        ShowInTaskbar = false,
                        Owner = this
                    };
                    _ = dialog.ShowDialog();
                    if (!dialog.Confirmed)
                    {
                        return;
                    }
                }

                if (action.IsConfirmed 
                    && MessageBox.Show($"确定要发送吗?\n{command}", "确认", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                _instance.Send(command);
            }
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            Window setting = new Setting
            {
                ShowInTaskbar = false,
                Owner = this
            };
            _ = setting.ShowDialog();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string command = CommandTextBox.Text;
            if (!string.IsNullOrWhiteSpace(command))
            {
                _instance.Send(command.Trim());
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Clear();
        }
    }
}