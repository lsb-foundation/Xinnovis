using CommonLib.Extensions;
using MFCSoftware.Utils;
using CommonLib.Utils;
using MFCSoftware.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace MFCSoftware.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel mainVm;
        private readonly LinkedList<ChannelUserControl> controlList = new LinkedList<ChannelUserControl>();

        private LinkedListNode<ChannelUserControl> currentNode;
        private Timer timer;
        private Task sendTask;

        private const int totalInterval = 1000;
        private const int minInterval = 100;      //最小的发送间隔时间固定为100毫秒
        
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            mainVm = this.DataContext as MainWindowViewModel;
            InitializeTimer();
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
            timer.Stop();
            timer.Dispose();

            ChannelGrid.Children.Clear();
            controlList.Clear();
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await SendTaskCompletedAsync();

            //定时发送的后台任务
            sendTask = Task.Run(async () =>
            {
                if (controlList.Count == 0)
                {
                    return;
                }

                currentNode = currentNode is null || currentNode == controlList.Last ?
                    controlList.First : currentNode.Next;

                if (currentNode?.Value != null)
                {
                    var channel = currentNode.Value;
                    if (await SendAsync(channel.ReadFlowBytes))
                    {
                        LoggerHelper.WriteLog("[Send] 读取流量 [Data] " + channel.ReadFlowBytes.Command.ToHexString());
                        try
                        {
                            byte[] data = await SerialPortInstance.GetResponseBytesAsync();
                            channel.ResolveData(data, channel.ReadFlowBytes.Type);
                        }
                        catch (TimeoutException)
                        {
                            channel.WhenTimeOut();
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.WriteLog(ex.Message, ex);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 异步等待当前发送任务执行结束
        /// </summary>
        /// <returns></returns>
        private async Task SendTaskCompletedAsync()
        {
            if (sendTask == null) return;
            if (!sendTask.IsCompleted)
            {
                timer.Stop();
                await sendTask;
                timer.Start();
            }
        }

        private async Task<bool> SendAsync(SerialCommand<byte[]> sc)
        {
            try
            {
                await SerialPortInstance.SendAsync(sc);
                return true;
            }
            catch(Exception e)
            {
                timer.Stop();
                mainVm.ShowMessage("串口可能被其他程序占用，请检查！");
                LoggerHelper.WriteLog(e.Message, e);
                return false;
            }
        }

        private void AddChannelButton_Click(object sender, RoutedEventArgs e)
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
                ChannelUserControl channelControl = new ChannelUserControl(window.Address);
                channelControl.ChannelClosed += ChannelControl_ControlWasRemoved;
                channelControl.SingleCommandSended += (channel, command) => SendSingleCommand(channel, command);

                ChannelGrid.Children.Add(channelControl);
                controlList.AddLast(channelControl);
                SetTimerInterval();
                ChannelAdded(channelControl);
                mainVm.ChannelCount++;
            }
            else mainVm.ShowMessage("地址重复，请重新添加。");
        }


        private void ChannelAdded(ChannelUserControl channel)
        {
            //添加通道后读取基本信息
            SendSingleCommand(channel, channel.ReadBaseInfoBytes);
        }

        private async void SendSingleCommand(ChannelUserControl channel, SerialCommand<byte[]> command)
        {
            await SendTaskCompletedAsync();

            timer.Stop();
            if (await SendAsync(command))
            {
                try
                {
                    byte[] data = await SerialPortInstance.GetResponseBytesAsync();
                    channel.ResolveData(data, command.Type);
                }
                catch (TimeoutException)
                {
                    channel.WhenTimeOut();
                }
                catch(Exception ex)
                {
                    LoggerHelper.WriteLog(ex.Message, ex);
                }
                finally
                {
                    timer.Start();
                }
            }
        }

        private void ChannelControl_ControlWasRemoved(ChannelUserControl channel)
        {
            controlList.Remove(channel);
            ChannelGrid.Children.Remove(channel);
            mainVm.ChannelCount--;
            SetTimerInterval();
        }

        private void InitializeTimer()
        {
            timer = new Timer
            {
                Interval = totalInterval
            };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void SetTimerInterval()
        {
            if (controlList.Count == 0)
            {
                timer.Interval = totalInterval;
                return;
            }

            var interval = totalInterval / controlList.Count;
            if (interval < minInterval)
                interval = minInterval;
            timer.Interval = interval;
        }
    }
}
