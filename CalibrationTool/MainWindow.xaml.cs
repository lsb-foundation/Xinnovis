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
        private MainWindowViewModel mainViewModel;
        private SerialPortViewModel serialPortViewModel;
        private DebugViewModel debugViewModel;
        private ReadFlowViewModel readFlowViewModel;
        private StatusBarViewModel statusBarViewModel;
        #endregion

        private SerialPort serialPort;
        private ActionType currentAction;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMainViewModel();
            InitializeStatusBarViewModel();
            InitializeSerialPortViewModel();
            InitializeReadFlowViewModel();
            InitializeDebugViewModel();
        }

        #region 初始化ViewModel
        private void InitializeSerialPortViewModel()
        {
            serialPortViewModel = new SerialPortViewModel();
            serialPort = serialPortViewModel.GetSerialPortInstance();
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPortViewModel.MessageHandler = message => statusBarViewModel.ShowStatus(message);
            SerialPortTabItem.DataContext = serialPortViewModel;
        }

        private void InitializeReadFlowViewModel()
        {
            readFlowViewModel = new ReadFlowViewModel(() => 
            {
                Send(CommunicationDataType.Hex, new byte[1] { 0x90 });
                currentAction = ActionType.READ_FLOW;
            });
            ReadFlowGroupBox.DataContext = readFlowViewModel;
        }

        private void InitializeMainViewModel()
        {
            mainViewModel = new MainWindowViewModel();
            ContentGrid.DataContext = mainViewModel;
        }

        private void InitializeStatusBarViewModel()
        {
            statusBarViewModel = new StatusBarViewModel();
            AppStatusBar.DataContext = statusBarViewModel;
        }

        private void InitializeDebugViewModel()
        {
            debugViewModel = new DebugViewModel();
            debugViewModel.DebugCommand = new RelayCommand(
                o => 
                {
                    Send(CommunicationDataType.ASCII, "DEBUG!");
                    currentAction = ActionType.DEBUG;
                });
            DebugGroupBox.DataContext = debugViewModel;
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
                        ResolveStringData();
                        break;
                    case ActionType.DEBUG:
                        ResolveDebugData();
                        break;
                    case ActionType.READ_FLOW:
                        ResolveFlowData();
                        break;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                statusBarViewModel.Status = ex.Message;
            }
        }

        private void DisplayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayTextBox.ScrollToEnd();
        }

        #region 串口数据发送
        private void Send(CommunicationDataType type, object data)
        {
            if (!serialPort.IsOpen)
            {
                statusBarViewModel.ShowStatus("串口未打开");
                return;
            }

            try
            {
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
                statusBarViewModel.Status = e.Message;
            }
        }
        #endregion

        #region 数据解析
        private void ResolveStringData()
        {
            IResolve<string, string> strResolve = new StringDataResolve();
            string sourceData = serialPort.ReadLine();
            string resolvedData = strResolve.Resolve(sourceData);
            mainViewModel.AppendStringToBuilder(resolvedData);
        }

        private void ResolveDebugData()
        {
            IResolve<string, KeyValuePair<string, string>> debugResolve = new DebugDataResolve();
            string sourceData = serialPort.ReadLine();
            KeyValuePair<string, string> resolvedData = debugResolve.Resolve(sourceData);
            mainViewModel.AppendStringToBuilder(string.Format("{0}: {1}{2}", resolvedData.Key, resolvedData.Value, Environment.NewLine));
            debugViewModel.SetDataProperty(resolvedData);
        }

        private void ResolveFlowData()
        {
            IResolve<byte[], double> flowResolve = new FlowDataResolve();
            int count = serialPort.BytesToRead;
            byte[] sourceData = new byte[count];
            serialPort.Read(sourceData, 0, count);
            double resolvedData = flowResolve.Resolve(sourceData);
            mainViewModel.AppendStringToBuilder(String.Format("{0}{1}", resolvedData.ToString(), Environment.NewLine));
        }
        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
