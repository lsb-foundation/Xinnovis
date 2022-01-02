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
using System.Linq;

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

        private IActionHandler _actionHandler;

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
            string text = Encoding.Default.GetString(buffer);

            _actionHandler?.Receive(text);

            lock (_builder)
            {
                _builder.Append(text);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializePassword();

            var files = SettingViewModel.LoadAutoUiFiles();
            string file = Settings.Default.AutoUiFile;
            file = files.Contains(file) ? file : files.FirstOrDefault();
            if (!string.IsNullOrEmpty(file))
            {
                LoadAutoUi(file);
            }
            else
            {
                _ = MessageBox.Show("缺失autoui文件！");
            }
        }

        public void LoadAutoUi(string file)
        {
            var config = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./autoui", file);
            _main.SetAppTitleForConfig(file);

            Settings.Default.AutoUiFile = file;
            Settings.Default.Save();
            Settings.Default.Reload();

            AutoUIGrid.Children.Clear();
            AutoUI autoUI = AutoUI.GetUIAuto(config);
            autoUI.Executed += AutoUI_Executed;
            _ = AutoUIGrid.Children.Add(autoUI.Build());
        }

        private void AutoUI_Executed(IAutoBuilder build)
        {
            if (build is UIAction action)
            {
                (bool success, string message) = action.TryParse();
                if (!success)
                {
                    _ = MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (action.IsConfirmed
                    && MessageBox.Show($"确定要发送吗?\n{action.Command}", "确认", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                if (action.IsAuthorized)
                {
                    ConfirmPasswordDialog dialog = new()
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
                
                if (!string.IsNullOrWhiteSpace(action.Handler))
                {
                    _actionHandler = action.CreateHandler();
                    if (_actionHandler != null)
                    {
                        _actionHandler.Completed += () => _actionHandler = null;
                        _actionHandler.Initialize();
                        _main.Instance.Send(action.Command);
                        _actionHandler.Execute();
                    }
                    else
                    {
                        _ = MessageBox.Show("Handler配置错误，请检查！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    _main.Instance.Send(action.Command);
                }
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