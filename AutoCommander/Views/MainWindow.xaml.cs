using System;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Windows;
using AutoCommander.Properties;
using AutoCommander.UIModels;
using AutoCommander.ViewModels;
using CommonLib.Extensions;
using System.Windows.Threading;

namespace AutoCommander.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _main;
        private readonly DispatcherTimer _dispatcherTimer;  //更新UI的Timer
        private readonly StringBuilder _builder;

        public MainWindow()
        {
            InitializeComponent();

            _main = DataContext as MainViewModel;
            _main.Instance.Port.DataReceived += SerialPort_DataReceived;

            _builder = new StringBuilder();
            _dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            string text = null;
            lock (_builder)
            {
                text = _builder.ToString();
                _ = _builder.Clear();
            }
            if (!string.IsNullOrEmpty(text))
            {
                ResultTextBox.AppendText(text);
                ResultTextBox.ScrollToEnd();
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = _main.Instance.Port;
            byte[] buffer = new byte[port.BytesToRead];
            _ = port.Read(buffer, 0, buffer.Length);
            string chars = Encoding.Default.GetString(buffer);

            lock (_builder)
            {
                _ = _builder.Append(chars);
            }
            //await Task.Run(() => LoggerHelper.WriteLog($"Received: {chars}"));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializePassword();
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
                _main.Instance.Send(command);
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

        private void InitializePassword()
        {
            if (Settings.Default.Password == "123456")
            {
                Settings.Default.Password = "123456".MD5HashString();
                Settings.Default.Save();
                Settings.Default.Reload();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Clear();
        }
    }
}