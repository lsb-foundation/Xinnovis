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
                    Send(CommunicationDataType.ASCII, "DEBUG!");
                    currentAction = ActionType.DEBUG;
                });
            reader.SendCaliCommand = new RelayCommand(o =>
                {
                    Send(CommunicationDataType.ASCII, "CALI!");
                    currentAction = ActionType.CALI;
                });
            reader.SetReadFlowCommand(() =>
            {
                Send(CommunicationDataType.Hex, new byte[1] { 0x90 });
                currentAction = ActionType.READ_FLOW;
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
            WriteDataTabItem.DataContext = writer;
        }
        #endregion

        /// <summary>
        /// 串口数据接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                switch (currentAction)
                {
                    case ActionType.CALI:
                    case ActionType.VOLT:
                    case ActionType.K:
                    case ActionType.GAS:
                        ResolveStringData();
                        break;
                    case ActionType.DEBUG:
                        ResolveDebugData();
                        break;
                    case ActionType.READ_FLOW:
                        ResolveFlowData();
                        break;
                    case ActionType.Custom:
                        ResolveCustomData();
                        break;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                statusVm.Status = ex.Message;
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
                statusVm.Status = e.Message;
            }
        }

        private void SendCustomData()
        {

        }
        #endregion

        #region 数据解析
        private void ResolveStringData()
        {
            IResolve<string, string> strResolve = new StringDataResolve();
            string sourceData = serialPort.ReadLine();
            string resolvedData = strResolve.Resolve(sourceData);
            main.AppendStringToBuilder(resolvedData.Trim() + Environment.NewLine);
        }

        private void ResolveDebugData()
        {
            IResolve<string, KeyValuePair<string, string>> debugResolve = new DebugDataResolve();
            string sourceData = serialPort.ReadLine();
            KeyValuePair<string, string> resolvedData = debugResolve.Resolve(sourceData);
            main.AppendStringToBuilder(string.Format("{0}: {1}{2}", resolvedData.Key, resolvedData.Value, Environment.NewLine));
            reader.SetDebugData(resolvedData);
        }

        private void ResolveFlowData()
        {
            IResolve<byte[], double> flowResolve = new FlowDataResolve();
            int count = serialPort.BytesToRead;
            byte[] sourceData = new byte[count];
            serialPort.Read(sourceData, 0, count);
            double resolvedData = flowResolve.Resolve(sourceData);
            main.AppendStringToBuilder(String.Format("{0}{1}", resolvedData.ToString(), Environment.NewLine));
        }

        private void ResolveCustomData()
        {

        }
        #endregion
    }
}
