﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CalibrationTool.ViewModels;
using CalibrationTool.Models;
using CommonLib.Communication.Serial;
using CalibrationTool.ResolveUtils;
using Panuon.UI.Silver;
using CommonLib.Extensions;
using CalibrationTool.UIAuto;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Text;
using GalaSoft.MvvmLight.Command;

namespace CalibrationTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowX
    {
        #region ViewModel定义
        private MainWindowViewModel main;
        private SerialPortViewModel serialVm;
        private ReadDataViewModel reader;
        private WriteDataViewModel writer;
        private StatusBarViewModel statusVm;
        #endregion

        private AdvancedSerialPort serialPort;
        private ActionType currentAction;
        private readonly StringBuilder _debugContentBuilder = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
            InitializeMainViewModel();
            InitializeStatusBarViewModel();
            InitializeSerialPortViewModel();
            InitializeReadDataViewModel();
            InitializeWriteDataViewModel();
        }

        #region 初始化ViewModel
        private void InitializeSerialPortViewModel()
        {
            serialVm = ViewModelLocator.Locator.GetInstance<SerialPortViewModel>();
            serialPort = serialVm.GetSerialPortInstance();
            serialPort.ReceivedDataHandler = data => ResolveData(data);
            serialVm.MessageHandler = message => statusVm.ShowStatus(message);
            SerialPortTabItem.DataContext = serialVm;
        }

        private void InitializeMainViewModel()
        {
            main = ViewModelLocator.Locator.GetInstance<MainWindowViewModel>();
            main.AppendTextToDisplayAction = text => this.Dispatcher.Invoke(() => DisplayTextBox.AppendText(text));
            main.ClearDisplayCommand = new RelayCommand(
                () => this.Dispatcher.Invoke(() => DisplayTextBox.Clear()));
            main.CopyDisplayContentCommand = new RelayCommand(
                () => this.Dispatcher.Invoke(() =>
                {
                    if (string.IsNullOrWhiteSpace(DisplayTextBox.Text)) return;
                    Clipboard.SetText(DisplayTextBox.Text);
                }));
            main.SendCommand = new RelayCommand(() =>
            {
                SendCustomData();
                currentAction = ActionType.Custom;
            });
            this.DataContext = main;
            ContentGrid.DataContext = main;
        }

        private void InitializeStatusBarViewModel()
        {
            statusVm = ViewModelLocator.Locator.GetInstance<StatusBarViewModel>();
            AppStatusBar.DataContext = statusVm;
        }

        private void InitializeReadDataViewModel()
        {
            reader = ViewModelLocator.Locator.GetInstance<ReadDataViewModel>();
            reader.SendDebugCommand = new RelayCommand(() => 
            {
                _debugContentBuilder.Clear();
                Send(CommunicationDataType.ASCII, reader.DebugCommand, ActionType.DEBUG);
            });
            reader.SetReadFlowCommand(() => Send(CommunicationDataType.Hex, reader.FlowCommand, ActionType.READ_FLOW));
            reader.SendCaliCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, reader.CaliCommand));
            reader.SendAVStartCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, reader.GetAVStartCommand()));
            reader.SendAVStopCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, reader.AVStopCommand));
            reader.SendCheckAVStartCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, reader.GetCheckAVStartCommand()));
            reader.SendCheckStopCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, reader.CheckStopCommand));
            reader.SendAIStartCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, reader.GetAIStartCommand()));
            reader.SendAIStopCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, reader.AIStopCommand));
            reader.SendCheckAIStartCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, reader.GetCheckAIStartCommand()));
            reader.SendPWMTestStartCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, reader.PWMTestStartCommand));
            reader.SendPWMTestStopCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, reader.PWMTestStopCommand));
            DebugTabItem.DataContext = reader;
            ReadDataTabItem.DataContext = reader;
        }

        private void InitializeWriteDataViewModel()
        {
            writer = ViewModelLocator.Locator.GetInstance<WriteDataViewModel>();
            writer.SendVoltCommand = new RelayCommand(() =>
            {
                if (string.IsNullOrWhiteSpace(writer.VoltCommand)) return;
                Send(CommunicationDataType.ASCII, writer.VoltCommand);
            });
            writer.SendKCommand = new RelayCommand(() =>
            {
                if (string.IsNullOrWhiteSpace(writer.KCommand)) return;
                Send(CommunicationDataType.ASCII, writer.KCommand);
            });
            writer.SetGasCommand = new RelayCommand(() =>
            {
                if (writer.Range <= 0) return;
                Send(CommunicationDataType.ASCII, writer.GetGasCommand());
            });
            writer.SetTemperatureCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, writer.GetTemperatureCommand()));
            writer.SetAvCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, writer.GetAvCommand()));
            writer.SetAiCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, writer.GetAiCommand()));
            writer.SetPWMCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, writer.GetPWMCommand()));
            writer.SendClearEEPRomCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, writer.ClearEEPRomCommand));
            writer.SetGasFactorCommand = new RelayCommand(() => Send(CommunicationDataType.ASCII, writer.GetGasFactorCommand()));
            writer.MessageHandler = message => statusVm.ShowStatus(message);

            KVoltDataItem.DataContext = writer;
            WriteDataTabItem.DataContext = writer;
        }
        #endregion

        private void DisplayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayTextBox.ScrollToEnd();
        }

        #region 串口数据发送
        private void Send(CommunicationDataType type, object data, ActionType currentActionType = ActionType.Default)
        {
            try
            {
                serialVm.TryToOpenPort();
                switch (type)
                {
                    case CommunicationDataType.ASCII:
                        serialPort.Write(data as string);
                        break;
                    case CommunicationDataType.Hex:
                        byte[] arrayToSend = data as byte[];
                        serialPort.Write(arrayToSend, 0, arrayToSend.Length);
                        break;
                    default:
                        return;
                }
                currentAction = currentActionType;
            }
            catch(Exception e)
            {
                statusVm.ShowStatus("发送数据时发生异常：" + e.Message);
            }
        }

        private void SendCustomData()
        {
            if (string.IsNullOrWhiteSpace(main.CodeToSend)) return;

            try
            {
                if (serialVm.SendType == CommunicationDataType.ASCII)
                    Send(CommunicationDataType.ASCII, main.CodeToSend);
                else if (serialVm.SendType == CommunicationDataType.Hex)
                {
                    byte[] bytesToSend = main.CodeToSend.HexStringToBytes();
                    Send(CommunicationDataType.Hex, bytesToSend);
                }
            }
            catch(Exception e)
            {
                statusVm.ShowStatus("错误：" + e.Message);
            }
        }
        #endregion

        #region 数据解析
        private void ResolveData(IList<byte> data)
        {
            switch (currentAction)
            {
                case ActionType.Default:
                case ActionType.AutoGen:
                    ResolveStringData(data.ToArray());
                    break;
                case ActionType.DEBUG:
                    ResolveDebugData(data.ToArray());
                    break;
                case ActionType.READ_FLOW:
                    ResolveFlowData(data.ToArray());
                    break;
                case ActionType.Custom:
                    ResolveCustomData(data.ToArray());
                    break;
                default:
                    return;
            }
        }

        private void ResolveStringData(byte[] data)
        {
            IResolve<byte[], string> strResolve = new StringDataResolve();
            string resolvedData = strResolve.Resolve(data);
            main.AppendTextToDisplay(resolvedData.Trim() + Environment.NewLine);
        }

        private void ResolveDebugData(byte[] data)
        {
            IResolve<string, List<KeyValuePair<string, string>>> debugResolve = new DebugDataResolve();
            string content = new StringDataResolve().Resolve(data);
            _debugContentBuilder.Append(content);
            main.AppendTextToDisplay(content);
            List<KeyValuePair<string, string>> resolvedDatas = debugResolve.Resolve(_debugContentBuilder.ToString());
            foreach (KeyValuePair<string, string> pair in resolvedDatas)
            {
                reader.SetDebugData(pair);
            }
        }

        private void ResolveFlowData(byte[] data)
        {
            IResolve<byte[], double> flowResolve = new FlowDataResolve();
            double resolvedData = flowResolve.Resolve(data);
            main.AppendTextToDisplay(String.Format("{0}{1}", resolvedData.ToString(), Environment.NewLine));
        }

        private void ResolveCustomData(byte[] data)
        {
            string result = string.Empty;
            if (serialVm.ReceivedType == CommunicationDataType.Hex)
                result = data.ToHexString();
            else if(serialVm.ReceivedType == CommunicationDataType.ASCII)
            {
                IResolve<byte[], string> resolver = new StringDataResolve();
                result = resolver.Resolve(data);
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = serialVm.AutoAddNewLine ?
                    string.Format("{0}{1}", result.Trim(), Environment.NewLine) :
                    result.Trim();
                main.AppendTextToDisplay(result);
            }
        }
        #endregion

        private void WindowX_Loaded(object sender, RoutedEventArgs e)
        {
            if (ConfigurationManager.GetSection("UIAuto") is UIAutoSection section)
            {
                section.UIAutoActionInvoked += Section_UIAutoActionInvoked;
                if (section.Build() is TabControl tabControl)
                {
                    List<TabItem> tabs = new List<TabItem>();
                    foreach (TabItem tab in tabControl.Items)
                    {
                        tabs.Add(tab);
                    }
                    tabControl.Items.Clear();
                    tabs.ForEach(tab => MainTab.Items.Add(tab));
                }
            }
        }

        private void Section_UIAutoActionInvoked(UIAutoActionEventArgs e)
        {
            try
            {
                string convertedFormat = e.Action.Format;
                MatchCollection matches = Regex.Matches(e.Action.Format, @"{\w+}");
                foreach (Match match in matches)
                {
                    string parameterName = match.Value.Trim('{', '}');

                    if (!(e.Parameters.FirstOrDefault(p => p.Name == parameterName) is ParameterElement parameter))
                    {
                        throw new Exception($"参数{parameterName}未找到。");
                    }

                    if (string.IsNullOrWhiteSpace(parameter.Value))
                    {
                        throw new Exception($"参数{parameter.Description}为空。");
                    }

                    bool canParse = false;
                    object parsedValue = null;
                    switch (parameter.Type.Trim().ToLower())
                    {
                        case "int":
                            canParse = int.TryParse(parameter.Value, out int intNumber);
                            parsedValue = intNumber;
                            break;
                        case "float":
                            canParse = float.TryParse(parameter.Value, out float floatNumber);
                            parsedValue = floatNumber;
                            break;
                        case "string":
                            parsedValue = parameter.Value;
                            canParse = true;
                            break;
                        default:
                            throw new Exception($"输入参数[{parameter.Description}]的类型{parameter.Type}暂不支持。");
                    }

                    if (!canParse)
                    {
                        throw new Exception($"输入参数[{parameter.Description}]的类型不正确。");
                    }
                    convertedFormat = convertedFormat.Replace(match.Value, parsedValue.ToString());
                }

                if (e.Command.Type?.Trim().ToUpper() == "HEX")
                {
                    byte[] data = convertedFormat.HexStringToBytes();
                    Send(CommunicationDataType.Hex, data, ActionType.AutoGen);
                }
                else
                {
                    Send(CommunicationDataType.ASCII, convertedFormat, ActionType.AutoGen);
                }
            }
            catch (Exception ex)
            {
                statusVm.ShowStatus(ex.Message);
            }
        }
    }
}
