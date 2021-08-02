using MFCSoftware.ViewModels;
using System;
using System.Linq;
using System.Windows;
using CommonLib.Extensions;
using MFCSoftware.Common;
using System.Timers;
using MFCSoftware.Models;
using System.Reflection;
using CommonLib.Utils;
using CommonLib.MfcUtils;

namespace MFCSoftware.Views
{
    /// <summary>
    /// SetAddressWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetAddressWindow : Window
    {
        private readonly SetAddressWindowViewModel viewModel;
        private Timer timer;

        public SetAddressWindow()
        {
            InitializeComponent();
            InitializeWindow();
            viewModel = this.DataContext as SetAddressWindowViewModel;
        }

        private void InitializeWindow()
        {
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            viewModel.Enable = true;
            timer.Stop();
        }

        private void ResolveData(byte[] data, SerialCommandType type)
        {
            if (timer.Enabled) timer.Stop();
            
            viewModel.Enable = true;

            try
            {
                bool result = data.CheckCRC16ByDefault();
                if (result)
                {
                    if (type == SerialCommandType.ReadAddress)
                        result = HandleReadAddress(data);
                    else if (type == SerialCommandType.WriteAddress)
                        result = HandleWriteAdderss(data);
                    else if (type == SerialCommandType.SetBaudRate)
                        result = HandleSetBaudRate(data);
                }

                if (!result)
                {
                    MessageBox.Show("数据解析失败！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception e)
            {
                LoggerHelper.WriteLog(e.Message, e);
            }
        }

        private bool HandleReadAddress(byte[] data)
        {
            if (AutoCheck(data, SerialCommandType.ReadAddress))
            {
                viewModel.ReaderAddress = data[4];
                return true;
            }
            return false;
        }

        private bool HandleWriteAdderss(byte[] data)
        {
            if (AutoCheck(data, SerialCommandType.WriteAddress))
            {
                viewModel.WriterAddress = data[4];
                return true;
            }
            return false;
        }

        private bool HandleSetBaudRate(byte[] data)
        {
            if(AutoCheck(data, SerialCommandType.SetBaudRate))
            {
                uint intBaudcode = data[4];
                viewModel.BaudRateCode = viewModel.BaudRateCodes.FirstOrDefault(c => c.Code == intBaudcode);
                return true;
            }
            return false;
        }

        private bool AutoCheck(byte[] data, SerialCommandType type)
        {
            var resolveActionAttr = type.GetType().GetField(type.ToString()).GetCustomAttribute<ResolveActionAttribute>();
            if (resolveActionAttr == null) return false;
            return resolveActionAttr.Check(data, 0);
        }

        private void ReadAddressButton_Click(object sender, RoutedEventArgs e)
        {
            Send(viewModel.ReadAddressBytes);
        }

        private void SetAddressButton_Click(object sender, RoutedEventArgs e)
        {
            if(viewModel.WriterAddress < 1 || viewModel.WriterAddress > 250)
            {
                MessageBox.Show("请确保地址范围在1-250之间。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Send(viewModel.WriteAddressBytes);
        }

        private void SetBaudRateButton_Click(object sender, RoutedEventArgs e)
        {
            Send(viewModel.SetBaudRateBytes);
        }

        private async void Send(SerialCommand<byte[]> command)
        {
            try
            {
                await SerialPortInstance.SendAsync(command);
                viewModel.Enable = false;
                timer.Start();
                var data = await SerialPortInstance.GetResponseBytesAsync();
                ResolveData(data, command.Type);
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
    }
}
