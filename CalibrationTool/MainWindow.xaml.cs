using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CalibrationTool.ViewModels;
using CalibrationTool.Models;
using CommonLib.Communication;
using CommonLib.Mvvm;
using CalibrationTool.ResolveUtils;
using Panuon.UI.Silver;
using System.Collections.Specialized;
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

        private SerialPort serialPort;
        private ActionType currentAction;
        private bool receiving = false;     //串口是否正在接收数据

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
            serialPort.DataReceived += SerialPort_DataReceived;
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
            reader.SendDebugCommand = new RelayCommand(o =>
            {
                Send(CommunicationDataType.ASCII, reader.DebugCommand);
                currentAction = ActionType.DEBUG;
            });
            reader.SendCaliCommand = new RelayCommand(o =>
            {
                Send(CommunicationDataType.ASCII, reader.CaliCommand);
                currentAction = ActionType.CALI;
            });
            reader.SetReadFlowCommand(() =>
            {
                Send(CommunicationDataType.Hex, reader.FlowCommand);
                currentAction = ActionType.READ_FLOW;
            });
            reader.SendRefStartCommand = new RelayCommand(o =>
            {
                Send(CommunicationDataType.ASCII, reader.GetRefStartCommand());
                currentAction = ActionType.REF;
            });
            reader.SendRefStopCommand = new RelayCommand(o =>
            {
                Send(CommunicationDataType.ASCII, reader.RefStopCommand);
                currentAction = ActionType.REF;
            });
            ReadDataTabItem.DataContext = reader;
        }

        private void InitializeWriteDataViewModel()
        {
            writer = new WriteDataViewModel();
            writer.SendVoltCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(writer.VoltCommand)) return;
                Send(CommunicationDataType.ASCII, writer.VoltCommand);
                currentAction = ActionType.VOLT;
            });
            writer.SendKCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(writer.KCommand)) return;
                Send(CommunicationDataType.ASCII, writer.KCommand);
                currentAction = ActionType.K;
            });
            writer.SetGasCommand = new RelayCommand(o =>
            {
                if (writer.Range <= 0) return;
                Send(CommunicationDataType.ASCII, writer.GetGasCommand());
                currentAction = ActionType.GAS;
            });
            writer.MessageHandler = message => statusVm.ShowStatus(message);
            WriteDataTabItem.DataContext = writer;
        }
        #endregion

        /// <summary>
        /// 串口数据接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (receiving) return;

            receiving = true;
            List<byte> receivedBytes = new List<byte>();
            try
            {
                while (serialPort.BytesToRead > 0)
                {
                    int count = serialPort.BytesToRead;
                    byte[] data = new byte[count];
                    serialPort.Read(data, 0, count);
                    receivedBytes.AddRange(data);
                    await Task.Delay(10);
                }
                ResolveData(receivedBytes);
            }
            catch (Exception ex)
            {
                statusVm.ShowStatus("接受数据时发生异常: " + ex.Message);
            }
            finally
            {
                receivedBytes.Clear();
                receiving = false;
            }
        }

        private void DisplayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayTextBox.ScrollToEnd();
        }

        #region 串口数据发送
        private void Send(CommunicationDataType type, object data)
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
                case ActionType.CALI:
                case ActionType.VOLT:
                case ActionType.K:
                case ActionType.GAS:
                case ActionType.REF:
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
