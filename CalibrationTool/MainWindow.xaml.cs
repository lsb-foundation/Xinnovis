using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CalibrationTool.ViewModels;
using CalibrationTool.Models;
using CommonLib.Communication.Serial;
using CommonLib.Mvvm;
using CalibrationTool.ResolveUtils;
using Panuon.UI.Silver;
using CommonLib.Extensions;

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
            serialVm = new SerialPortViewModel();
            serialPort = serialVm.GetSerialPortInstance();
            serialPort.ReceivedDataHandler = data => ResolveData(data);
            serialVm.MessageHandler = message => statusVm.ShowStatus(message);
            SerialPortTabItem.DataContext = serialVm;
        }

        private void InitializeMainViewModel()
        {
            main = new MainWindowViewModel();
            main.AppendTextToDisplayAction = text => this.Dispatcher.Invoke(() => DisplayTextBox.AppendText(text));
            main.ClearDisplayCommand = new RelayCommand(
                o => this.Dispatcher.Invoke(() => DisplayTextBox.Clear()));
            main.CopyDisplayContentCommand = new RelayCommand(
                o => this.Dispatcher.Invoke(() =>
                {
                    if (string.IsNullOrWhiteSpace(DisplayTextBox.Text)) return;
                    Clipboard.SetText(DisplayTextBox.Text);
                }));
            main.SendCommand = new RelayCommand(o =>
            {
                SendCustomData();
                currentAction = ActionType.Custom;
            });
            this.DataContext = main;
            ContentGrid.DataContext = main;
        }

        private void InitializeStatusBarViewModel()
        {
            statusVm = new StatusBarViewModel();
            AppStatusBar.DataContext = statusVm;
        }

        private void InitializeReadDataViewModel()
        {
            reader = new ReadDataViewModel();
            reader.SendDebugCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.DebugCommand, ActionType.DEBUG));
            reader.SetReadFlowCommand(() => Send(CommunicationDataType.Hex, reader.FlowCommand, ActionType.READ_FLOW));
            reader.SendCaliCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.CaliCommand));
            reader.SendAVStartCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.GetAVStartCommand()));
            reader.SendAVStopCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.AVStopCommand));
            reader.SendCheckAVStartCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.GetCheckAVStartCommand()));
            reader.SendCheckStopCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.CheckStopCommand));
            reader.SendAIStartCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.GetAIStartCommand()));
            reader.SendAIStopCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.AIStopCommand));
            reader.SendCheckAIStartCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.GetCheckAIStartCommand()));
            reader.SendPWMTestStartCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.PWMTestStartCommand));
            reader.SendPWMTestStopCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, reader.PWMTestStopCommand));
            DebugTabItem.DataContext = reader;
            ReadDataTabItem.DataContext = reader;
        }

        private void InitializeWriteDataViewModel()
        {
            writer = new WriteDataViewModel();
            writer.SendVoltCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(writer.VoltCommand)) return;
                Send(CommunicationDataType.ASCII, writer.VoltCommand);
            });
            writer.SendKCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(writer.KCommand)) return;
                Send(CommunicationDataType.ASCII, writer.KCommand);
            });
            writer.SetGasCommand = new RelayCommand(o =>
            {
                if (writer.Range <= 0) return;
                Send(CommunicationDataType.ASCII, writer.GetGasCommand());
            });
            writer.SetTemperatureCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, writer.GetTemperatureCommand()));
            writer.SetAvCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, writer.GetAvCommand()));
            writer.SetAiCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, writer.GetAiCommand()));
            writer.SetPWMCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, writer.GetPWMCommand()));
            writer.SendClearEEPRomCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, writer.ClearEEPRomCommand));
            writer.SetGasFactorCommand = new RelayCommand(o => Send(CommunicationDataType.ASCII, writer.GetGasFactorCommand()));
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
            IResolve<byte[], List<KeyValuePair<string, string>>> debugResolve = new DebugDataResolve();
            List<KeyValuePair<string, string>> resolvedDatas = debugResolve.Resolve(data);
            foreach(KeyValuePair<string,string> pair in resolvedDatas)
            {
                main.AppendTextToDisplay(string.Format("{0}: {1}{2}", pair.Key, pair.Value, Environment.NewLine));
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
    }
}
