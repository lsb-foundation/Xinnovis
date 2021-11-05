using CommonLib.Extensions;
using MFCSoftware.Utils;
using CommonLib.Utils;
using MFCSoftware.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MFCSoftware.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel mainVm;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly LinkedList<ChannelUserControl> controlList = new LinkedList<ChannelUserControl>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            mainVm = this.DataContext as MainWindowViewModel;

            StartLoopToSend();
        }

        private void OpenSetSerialPortWindow(object sender, EventArgs e)
        {
            SetSerialPortWindow window = new SetSerialPortWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ShowInTaskbar = false
            };
            window.ShowDialog();
        }

        private void OpenSetAddressWindow(object sender, EventArgs e)
        {
            SetAddressWindow window = new SetAddressWindow
            {
                Owner = this,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            window.ShowDialog();
        }

        //关闭窗口，清理资源
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _cancel.Cancel();
            ChannelGrid.Children.Clear();
            controlList.Clear();
        }

        //todo: 完善线程发送机制
        //todo: 退出保存设置
        private void StartLoopToSend()
        {
            Task.Factory.StartNew(async () =>
            {
                LinkedListNode<ChannelUserControl> currentNode = null;
                while (!_cancel.IsCancellationRequested && controlList.Count > 0)
                {
                    currentNode = currentNode is null || currentNode == controlList.Last ?
                        controlList.First : currentNode.Next;
                    if (currentNode?.Value != null)
                    {
                        var channel = currentNode.Value;
                        await SendResolveAsync(channel, channel.ReadFlowBytes);
                    }
                    Thread.Sleep(10);
                }
            }, _cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async void AddChannelButton_Click(object sender, RoutedEventArgs e)
        {
            AddChannelWindow window = new AddChannelWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ShowInTaskbar = false
            };

            window.ShowDialog();

            if (!window.IsAddressSetted) return;

            bool addrExists = controlList.Any(c => c.Address == window.Address);
            if (!addrExists)
            {
                ChannelUserControl channel = new ChannelUserControl(window.Address);
                channel.ChannelClosed += ChannelControl_ControlWasRemoved;
                channel.SingleCommandSended += async (chn, cmd) => await SendResolveAsync(chn, cmd);

                ChannelGrid.Children.Add(channel);
                controlList.AddLast(channel);
                await SendResolveAsync(channel, channel.ReadBaseInfoBytes); //添加通道后读取基本信息
                mainVm.ChannelCount++;
            }
            else mainVm.ShowMessage("地址重复，请重新添加。");
        }

        private async Task SendResolveAsync(ChannelUserControl channel, SerialCommand<byte[]> command)
        {
            try
            {
                await _semaphore.WaitAsync();
                await SerialPortInstance.SendAsync(command);
                LoggerHelper.WriteLog("[Send] 读物流量 [Data] " + channel.ReadFlowBytes.Command.ToHexString());
                byte[] data = await SerialPortInstance.GetResponseBytesAsync();
                channel.ResolveData(data, command.Type);
            }
            catch (TimeoutException)
            {
                channel.WhenTimeOut();
            }
            catch (Exception e)
            {
                mainVm.ShowMessage("发生异常: " + e.Message);
                LoggerHelper.WriteLog(e.Message, e);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void ChannelControl_ControlWasRemoved(ChannelUserControl channel)
        {
            controlList.Remove(channel);
            ChannelGrid.Children.Remove(channel);
            mainVm.ChannelCount--;
        }
    }
}
