using MFCSoftware.Common;
using MFCSoftware.Models;
using MFCSoftware.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace MFCSoftware
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
            mainVm = new MainWindowViewModel();
            this.DataContext = mainVm;
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
                if (controlList.Count == 0) return;

                if (currentNode == null || currentNode == controlList.Last)
                    currentNode = controlList.First;
                else currentNode = currentNode.Next;

                if (currentNode?.Value != null)
                {
                    var channel = currentNode.Value;
                    if (Send(channel.ReadFlowBytes))
                    {
                        try
                        {
                            byte[] data = await SerialPortInstance.GetResponseBytes();
                            channel.ResolveData(data, ResolveType.FlowData);
                        }
                        catch (TimeoutException)
                        {
                            channel.WhenTimeOut();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog(ex.Message, ex);
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
                await sendTask;
        }

        private bool Send(SerialCommand<byte[]> sc)
        {
            try
            {
                SerialPortInstance.Send(sc);
                return true;
            }
            catch(Exception e)
            {
                timer.Stop();
                MessageBox.Show("串口可能被其他程序占用，请检查！");
                LogHelper.WriteLog(e.Message, e);
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
                ChannelUserControl channelControl = new ChannelUserControl();
                channelControl.SetAddress(window.Address);
                channelControl.ControlWasRemoved += ChannelControl_ControlWasRemoved;
                channelControl.ClearAccuFlowClicked += ChannelControl_ClearAccuFlowClicked;

                ChannelGrid.Children.Add(channelControl);
                controlList.AddLast(channelControl);
                SetTimerInterval();
                ChannelAdded(channelControl);
                mainVm.ChannelCount++;
            }
            else MessageBox.Show("地址重复，请重新添加。");
        }

        private async void ChannelAdded(ChannelUserControl channel)
        {
            //添加通道后读取基本信息
            await SendTaskCompletedAsync();
            SendSingleCommand(channel, channel.ReadBaseInfoBytes, ResolveType.BaseInfoData);
        }

        private async void ChannelControl_ClearAccuFlowClicked(ChannelUserControl channel)
        {
            await SendTaskCompletedAsync();
            SendSingleCommand(channel, channel.ClearAccuFlowBytes, ResolveType.ClearAccuFlowData);
        }

        private async void SendSingleCommand(ChannelUserControl channel, SerialCommand<byte[]> command, ResolveType type)
        {
            await SendTaskCompletedAsync();

            timer.Stop();
            if (Send(command))
            {
                try
                {
                    byte[] data = await SerialPortInstance.GetResponseBytes();
                    channel.ResolveData(data, type);
                }
                catch (TimeoutException)
                {
                    channel.WhenTimeOut();
                }
                catch(Exception ex)
                {
                    LogHelper.WriteLog(ex.Message, ex);
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
            timer = new Timer();
            timer.Interval = totalInterval;
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
