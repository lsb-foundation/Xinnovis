using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
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
        private readonly MainViewModel _main;
        public MainWindow()
        {
            InitializeComponent();
            _main = DataContext as MainViewModel;
            _main.Instance.SerialPort.DataReceived += SerialPort_DataReceived;
        }

        private async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = _main.Instance.SerialPort;
            string chars = port.ReadExisting();
            await Dispatcher.InvokeAsync(() =>
            {
                ResultTextBox.AppendText(chars);
                ResultTextBox.ScrollToEnd();
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCommandsCombox();
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

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string command = _main.EditableCommand?.Trim();
            if (!string.IsNullOrWhiteSpace(command))
            {
                _main.Instance.Send(command);
                if (!_main.LatestCommands.Contains(command))
                {
                    _main.LatestCommands.Insert(0, command);
                }
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"./commands.txt");
                using (FileStream stream = new FileStream(file, FileMode.Append, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    await writer.WriteLineAsync(command);
                }
            }
        }

        private async void InitializeCommandsCombox()
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"./commands.txt");
            using (FileStream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                List<string> commands = new List<string>();
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        commands.Add(line);
                    }
                }
                IEnumerable<string> orderComamnds = from c in commands
                                                    group c by c into g
                                                    select (command: g.Key, count: g.Count()) into cc
                                                    orderby cc.count descending
                                                    select cc.command;
                foreach (string command in orderComamnds.Take(10))
                {
                    _main.LatestCommands.Add(command);
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Clear();
        }
    }
}