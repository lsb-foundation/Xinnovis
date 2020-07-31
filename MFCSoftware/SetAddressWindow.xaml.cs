using MFCSoftware.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using CommonLib.Extensions;
using CommonLib.Communication.Serial;
using MFCSoftware.Common;
using CommonLib.Communication;
using System.Windows.Markup.Localizer;
using System.Security.Policy;
using System.Timers;

namespace MFCSoftware
{
    /// <summary>
    /// SetAddressWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetAddressWindow : Window
    {
        private SetAddressWindowViewModel viewModel;
        private AdvancedSerialPort serialPort;
        private Timer timer;

        private ActionType currentAction;

        public SetAddressWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            serialPort = AppSerialPortInstance.GetSerialPortInstance();
            serialPort.ReceivedDataHandler = ResolveData;
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            viewModel = new SetAddressWindowViewModel();
            this.DataContext = viewModel;
            this.Closed += SetAddressWindow_Closed;
        }

        private void SetAddressWindow_Closed(object sender, EventArgs e)
        {
            serialPort.ReceivedDataHandler = null;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            viewModel.Enable = true;
            timer.Stop();
        }

        private void ResolveData(byte[] data)
        {
            if (timer.Enabled) timer.Stop();
            
            viewModel.Enable = true;

            bool result;
            if (currentAction == ActionType.ReadAddress)
                result = HandleReadAddress(data);
            else if (currentAction == ActionType.WriteAddress)
                result = HandleWriteAdderss(data);
            else result = HandleSetBaudRate(data);

            if (!result)
            {
                MessageBox.Show("数据解析失败！");
            }
        }

        private bool HandleReadAddress(byte[] data)
        {
            if (!data.CheckCRC16ByDefault()) return false;

            //0xFE 0x03 0x02 0x00 addr CRCL CRCH
            bool isHeaderCorrect = data[0] ==
                0xfe && data[1] == 0x03 && data[2] == 0x02 && data[3] == 0x00;
            if (!isHeaderCorrect) return false;

            byte[] addr = data.Where((_, index) => index > 3 && index < data.Length - 2).ToArray();
            uint intAddr = BitConverter.ToUInt32(addr, 0);
            viewModel.ReaderAddress = intAddr;

            return true;
        }

        private bool HandleWriteAdderss(byte[] data)
        {
            if (!data.CheckCRC16ByDefault()) return false;

            //0xFE 0x06 0x02 0x00 addr CRCL CRCH
            bool isHeaderCorrect = data[0] ==
                0xfe && data[1] == 0x06 && data[2] == 0x02 && data[3] == 0x00;
            if (!isHeaderCorrect) return false;

            byte[] addr = data.Where((_, index) => index > 3 && index < data.Length - 2).ToArray();
            uint intAddr = BitConverter.ToUInt32(addr, 0);
            viewModel.WriterAddress = intAddr;

            return true;
        }

        private bool HandleSetBaudRate(byte[] data)
        {
            if (!data.CheckCRC16ByDefault()) return false;

            //0xFE 0x06 0x02 0x00 baudcode CRCL CRCH
            bool isHeaderCorrect = data[0] ==
                0xfe && data[1] == 0x06 && data[2] == 0x02 && data[3] == 0x00;
            if (!isHeaderCorrect) return false;

            byte[] baudcode = data.Where((_, index) => index > 3 && index < data.Length - 2).ToArray();
            uint intBaudcode = BitConverter.ToUInt32(baudcode, 0);
            viewModel.BaudRateCode = viewModel.BaudRateCodes.FirstOrDefault(c => c.Code == intBaudcode);

            return true;
        }

        private void ReadAddressButton_Click(object sender, RoutedEventArgs e)
        {
            Send(viewModel.ReadAddressBytes, ActionType.ReadAddress);
        }

        private void SetAddressButton_Click(object sender, RoutedEventArgs e)
        {
            if(viewModel.WriterAddress < 1 || viewModel.WriterAddress > 250)
            {
                MessageBox.Show("请确保地址范围在1-250之间。");
                return;
            }
            Send(viewModel.WriteAddressBytes, ActionType.WriteAddress);
        }

        private void SetBaudRateButton_Click(object sender, RoutedEventArgs e)
        {
            Send(viewModel.SetBaudRateBytes, ActionType.SetBaudRate);
        }

        private void Send(byte[] data, ActionType act)
        {
            try
            {
                if (!serialPort.IsOpen)
                    serialPort.Open();

                serialPort.Write(data, 0, data.Length);
                currentAction = act;
                viewModel.Enable = false;
                timer.Start();
            }
            catch
            {
                MessageBox.Show("串口连接失败，请检查！");
            }
        }

        enum ActionType
        {
            ReadAddress,
            WriteAddress,
            SetBaudRate
        }
    }
}
