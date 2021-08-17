using System;
using System.IO;
using System.IO.Ports;
using System.Windows;
using AutoCommander.Properties;
using AutoCommander.UIModels;
using AutoCommander.ViewModels;
using CommonLib.Extensions;

namespace AutoCommander.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _main;
        public MainWindow()
        {
            InitializeComponent();
            _main = DataContext as MainViewModel;
            _main.Instance.Port.DataReceived += SerialPort_DataReceived;
        }

        private async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = _main.Instance.Port;
            string chars = port.ReadExisting();
            await Dispatcher.InvokeAsync(() =>
            {
                ResultTextBox.AppendText(chars);
                ResultTextBox.ScrollToEnd();
            });
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