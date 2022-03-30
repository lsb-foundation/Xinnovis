using System;
using System.Text;
using System.IO.Ports;
using System.Windows;
using AutoCommander.Common;
using AutoCommander.Properties;
using AutoCommander.AutoUI.Models;
using AutoCommander.ViewModels;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Linq;
using CommonLib.Extensions;
using HandyControls = HandyControl.Controls;
using AutoCommander.AutoUI.Linkers;
using AutoCommander.Models;
using CommonLib.Utils;

namespace AutoCommander.Views;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _main;
    private readonly ViewModelLocator _locator;

    private readonly DispatcherTimer _dispatcherTimer;  //更新UI的Timer
    private readonly StringBuilder _builder;

    private IActionHandler actionHandler;
    private Linker linker;

    public MainWindow()
    {
        _locator = FindResource("Locator") as ViewModelLocator;
        InitializeComponent();
        _main = DataContext as MainViewModel;

        SerialPortInstance.Default.Instance.DataReceived += SerialPort_DataReceived;

        _builder = new StringBuilder();
        _dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
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

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        string command = _main.EditableCommand?.Trim();
        if (string.IsNullOrEmpty(command)) return;

        if (_main.SendDataType == DataType.Hex)
        {
            byte[] binData = command.HexStringToBytes();
            _locator.Serial.TrySend(binData);
        }
        else
        {
            if (_main.AutoSendingNewLine) command = $"{command.TrimEnd()}\r\n";
            SendCommandByASCII(command);
        }
        if (!_main.LatestCommands.Contains(command))
        {
            _main.LatestCommands.Insert(0, command);
        }
        await _main.WriteLatestCommand(command);
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort port = SerialPortInstance.Default.Instance;
        byte[] buffer = new byte[port.BytesToRead];
        _ = port.Read(buffer, 0, buffer.Length);

        string originText = Encoding.Default.GetString(buffer);
        string text = _main.ReceiveDataType switch
        {
            DataType.ASCII => originText,
            DataType.Hex => buffer.ToHexString(),
            _ => null
        };

        actionHandler?.Receive(originText);

        if (text is null) return;
        lock (_builder)
        {
            _builder.Append(text);
        }

        LoggerHelper.WriteLog("[REC]" + text);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        linker = Linker.CreateLinker();

        string file = Settings.Default.AutoUiFile;
        if (!_locator.Configuration.AutoUiFiles.Contains(file))
        {
            file = _locator.Configuration.AutoUiFiles.FirstOrDefault();
        }
        if (!string.IsNullOrEmpty(file))
        {
            _locator.Configuration.SelectedAutoUiFile = file;
            LoadAutoUi(file);
        }
        else
        {
            HandyControls.Growl.Warning("缺失autoui文件！");
        }
    }

    private void LoadAutoUi(string file)
    {
        var config = PathUtils.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.AutoUIFolder, file);
        _main.SetAppTitleForConfig(file);

        SettingsUtils.Save("AutoUiFile", file);

        AutoUI.Models.AutoUI autoUI = AutoUI.Models.AutoUI.GetUIAuto(config);
        MenuContainer.Items.Clear();
        MenuContainer.Tag = file;
        foreach (var tab in autoUI.Tabs)
        {
            MenuContainer.Items.Add(new ListBoxItem
            {
                Content = tab.Header,
                Tag = tab
            });
        }
        MenuContainer.SelectedIndex = 0;
    }

    private void AutoUI_Executed(IAutoBuilder build)
    {
        if (build is not UIAction action) return;

        action.CreateCommand();

        if (action.IsConfirmed
            && HandyControls.MessageBox.Show(
                messageBoxText: $"确定要发送吗?\n{action.Command}",
                caption: "确认",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question)
            != MessageBoxResult.Yes)
        {
            return;
        }

        SendCommandByASCII(action.Command);
    }

    private void SendCommandByASCII(string command)
    {
        if (linker?.Items?.FirstOrDefault(item => item.Command == command) is LinkerItem linkerItem
            && !string.IsNullOrWhiteSpace(linkerItem.Handler))
        {
            actionHandler = linkerItem.CreateHandler();
            actionHandler.Completed += () => Dispatcher.Invoke(new Action(() =>
            {
                actionHandler = null;
                _main.ClearAppStatus();
            }));
            actionHandler.Initialize();
            _locator.Serial.TrySend(linkerItem.Command);
            _main.SetAppStatus($"{linkerItem.Display()}");
        }
        else
        {
            _locator.Serial.TrySend(command);
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ResultTextBox.Clear();
    }

    private void MenuContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MenuContainer.SelectedItem is ListBoxItem { Tag: Tab tab }
            && tab.Build() is StackPanel panel
            && TabContainer.Content != panel)
        {
            tab.Executed += AutoUI_Executed;
            TabContainer.Content = panel;
        }
        e.Handled = true;
    }

    private void Configuration_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { SelectedItem: string file }
            && MenuContainer?.Tag is string oldFile
            && file != oldFile)
        {
            LoadAutoUi(file);
        }
        e.Handled = true;
    }

    private void ShowHandlerButton_Click(object sender, RoutedEventArgs e)
    {
        _locator.Configuration.SetLinker(linker);
        HandyControls.Dialog.Show<HandlerViewer>("BEB7FC");
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(ResultTextBox.Text);
    }
}
