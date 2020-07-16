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

namespace CalibrationTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region ViewModel定义
        private MainWindowViewModel mainViewModel;
        private SerialPortViewModel serialPortViewModel;
        private DebugViewModel debugViewModel;
        private ReadFlowViewModel readFlowViewModel;
        #endregion

        private SerialPort serialPort;
        private ActionType currentAction;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMainViewModel();
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
            SerialPortTabItem.DataContext = serialPortViewModel;
        }

        private void InitializeReadFlowViewModel()
        {
            readFlowViewModel = new ReadFlowViewModel();
            ReadFlowTabItem.DataContext = readFlowViewModel;
        }

        private void InitializeMainViewModel()
        {
            mainViewModel = new MainWindowViewModel();
            ContentGrid.DataContext = mainViewModel;
        }

        private void InitializeDebugViewModel()
        {
            debugViewModel = new DebugViewModel();
            debugViewModel.DebugCommand = new RelayCommand(
                o => Send(CommunicationDataType.ASCII, "DEBUG!"));
            DebugTabItem.DataContext = debugViewModel;
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
                    case ActionType.DEBUG:
                        IParse<string, string> asciiParse = new ASCIIStringDataParse();
                        string sourceData = serialPort.ReadLine();
                        string parsedData = asciiParse.Resolve(sourceData);
                        mainViewModel.AppendStringToBuilder(parsedData);
                        break;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                mainViewModel.Status = ex.Message;
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
                switch (type)
                {
                    case CommunicationDataType.ASCII:
                        SendASCIIString(data as string);
                        break;
                    case CommunicationDataType.Hex:
                        SendByteArray(data as byte[]);
                        break;
                    default:
                        return;
                }
            }
            catch(Exception e)
            {
                mainViewModel.Status = e.Message;
            }
        }

        private void SendASCIIString(string strToSend)
        {
            if (!serialPort.IsOpen)
                serialPort.Open();
            serialPort.Write(strToSend);
        }

        private void SendByteArray(byte[] arrayToSend)
        {
            if (!serialPort.IsOpen)
                serialPort.Open();
            serialPort.Write(arrayToSend, 0, arrayToSend.Length);
        }

        private void DebugButton_Clicked(object sender, RoutedEventArgs e)
        {
            Send(CommunicationDataType.ASCII, "DEBUG!");
            currentAction = ActionType.DEBUG;
        }
        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
