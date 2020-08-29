using MFCSoftware.ViewModels;
using System;
using System.Linq;
using System.Windows;
using CommonLib.Extensions;
using MFCSoftware.Common;
using System.Timers;
using MFCSoftware.Models;

namespace MFCSoftware.Views
{
    /// <summary>
    /// SetAddressWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetAddressWindow : Window
    {
        private SetAddressWindowViewModel viewModel;
        private Timer timer;

        private ActionType currentAction;

        public SetAddressWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            viewModel = new SetAddressWindowViewModel();
            this.DataContext = viewModel;
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

            try
            {
                bool result;
                if (currentAction == ActionType.ReadAddress)
                    result = HandleReadAddress(data);
                else if (currentAction == ActionType.WriteAddress)
                    result = HandleWriteAdderss(data);
                else result = HandleSetBaudRate(data);

                if (!result)
                {
                    MessageBox.Show("数据解析失败！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception e)
            {
                LogHelper.WriteLog(e.Message, e);
            }
        }

        private bool HandleReadAddress(byte[] data)
        {
            if (!data.CheckCRC16ByDefault()) return false;

            //0xFE 0x03 0x02 0x00 addr CRCL CRCH
            bool isHeaderCorrect = data[0] ==
                0xfe && data[1] == 0x03 && data[2] == 0x02 && data[3] == 0x00;

            if (!isHeaderCorrect) return false;

            viewModel.ReaderAddress = data[4];
            return true;
        }

        private bool HandleWriteAdderss(byte[] data)
        {
            if (!data.CheckCRC16ByDefault()) return false;

            //0xFE 0x06 0x02 0x00 addr CRCL CRCH
            bool isHeaderCorrect = data[0] ==
                0xfe && data[1] == 0x06 && data[2] == 0x02 && data[3] == 0x00;

            if (!isHeaderCorrect) return false;

            viewModel.WriterAddress = data[4];
            return true;
        }

        private bool HandleSetBaudRate(byte[] data)
        {
            if (!data.CheckCRC16ByDefault()) return false;

            //0xFE 0x06 0x02 0x00 baudcode CRCL CRCH
            bool isHeaderCorrect = data[0] ==
                0xfe && data[1] == 0x06 && data[2] == 0x02 && data[3] == 0x00;
            if (!isHeaderCorrect) return false;

            uint intBaudcode = data[4];
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
                MessageBox.Show("请确保地址范围在1-250之间。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Send(viewModel.WriteAddressBytes, ActionType.WriteAddress);
        }

        private void SetBaudRateButton_Click(object sender, RoutedEventArgs e)
        {
            Send(viewModel.SetBaudRateBytes, ActionType.SetBaudRate);
        }

        private async void Send(SerialCommand<byte[]> command, ActionType act)
        {
            try
            {
                SerialPortInstance.Send(command);
                currentAction = act;
                viewModel.Enable = false;
                timer.Start();
                var data = await SerialPortInstance.GetResponseBytes();
                ResolveData(data);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("接收数据超时。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("串口可能被其他程序占用，请检查", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
