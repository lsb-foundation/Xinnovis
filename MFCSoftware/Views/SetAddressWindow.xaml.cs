using MFCSoftware.ViewModels;
using System;
using System.Linq;
using System.Windows;
using CommonLib.Extensions;
using System.Timers;
using System.Reflection;
using CommonLib.Utils;
using MFCSoftware.Utils;

namespace MFCSoftware.Views
{
    /// <summary>
    /// SetAddressWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetAddressWindow : Window
    {
        private readonly SetAddressWindowViewModel viewModel;

        public SetAddressWindow()
        {
            InitializeComponent();
            viewModel = this.DataContext as SetAddressWindowViewModel;
        }

        private void ResolveData(byte[] data, SerialCommandType type)
        {
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
                viewModel.WriterAddress = data[5];
                return true;
            }
            return false;
        }

        private bool HandleSetBaudRate(byte[] data)
        {
            if(AutoCheck(data, SerialCommandType.SetBaudRate))
            {
                uint intBaudcode = data[5];
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
                viewModel.Enable = false;
                var data = await SerialPortInstance.GetResponseAsync(command);
                ResolveData(data, command.Type);
            }
            catch (TimeoutException)
            {
                ViewModelLocator.MainViewModel.ShowMessage("接收数据超时");
            }
            catch (Exception e)
            {
                LoggerHelper.WriteLog(e.Message, e);
                ViewModelLocator.MainViewModel.ShowMessage("串口可能被其他程序占用，请检查!");
            }
            finally
            {
                viewModel.Enable = true;
            }
        }
    }
}
