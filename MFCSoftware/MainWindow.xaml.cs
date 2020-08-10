using MFCSoftware.Common;
using MFCSoftware.Models;
using MFCSoftware.ViewModels;
using System;
using System.Collections.Generic;
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
        private MainWindowViewModel mainVm;

        private List<int> addedAddrs = new List<int>();
        private LinkedList<ChannelUserControl> controlList = new LinkedList<ChannelUserControl>();
        private LinkedListNode<ChannelUserControl> currentNode;
        private System.Timers.Timer timer;
        private Task sendTask;

        private int totalMilliSeconds = 1000;
        
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
            SetSerialPortWindow window = new SetSerialPortWindow();
            window.Owner = this;
            window.ShowInTaskbar = false;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }

        private void OpenSetAddressWindow(object sender, EventArgs e)
        {
            SetAddressWindow window = new SetAddressWindow();
            window.Owner = this;
            window.ShowInTaskbar = false;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }

        //关闭窗口，清理资源
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Dispose();

            ChannelGrid.Children.Clear();
            addedAddrs.Clear();
            controlList.Clear();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
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

        private bool Send(SerialCommand<byte[]> sc)
        {
            try
            {
                SerialPortInstance.Send(sc);
                return true;
            }
            catch
            {
                timer.Stop();
                MessageBox.Show("串口可能被其他程序占用，请检查！");
                return false;
            }
        }

        private void AddChannelButton_Click(object sender, RoutedEventArgs e)
        {
            AddChannelWindow window = new AddChannelWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.ShowInTaskbar = false;
            window.ShowDialog();

            if (window.IsAddressSetted)
            {
                if (!addedAddrs.Contains(window.Address))
                {
                    ChannelUserControl channelControl = new ChannelUserControl();
                    channelControl.SetAddress(window.Address);
                    channelControl.ControlWasRemoved += ChannelControl_ControlWasRemoved;
                    channelControl.ClearAccuFlowClicked += ChannelControl_ClearAccuFlowClicked;

                    ChannelGrid.Children.Add(channelControl);
                    SetTimerInterval();
                    addedAddrs.Add(window.Address);
                    controlList.AddLast(channelControl);
                    ChannelAdded(channelControl);
                    mainVm.ChannelCount++;
                }
                else MessageBox.Show("地址重复。");
            }
        }

        private async void ChannelControl_ClearAccuFlowClicked(ChannelUserControl channel)
        {
            if (!(bool)sendTask?.IsCompleted)   //等待当前发送任务完成
                await sendTask;

            timer.Stop();
            if (Send(channel.ClearAccuFlowBytes))
            {
                try
                {
                    byte[] data = await SerialPortInstance.GetResponseBytes();
                    channel.ResolveData(data, ResolveType.ClearAccuFlowData);
                    timer.Start();
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

        private void ChannelControl_ControlWasRemoved(ChannelUserControl sender)
        {
            timer.Stop();
            addedAddrs.Remove(sender.Address);
            controlList.Remove(sender);
            ChannelGrid.Children.Remove(sender);
            mainVm.ChannelCount--;

            SetTimerInterval();
            timer.Start();
        }

        private async void ChannelAdded(ChannelUserControl channel)
        {
            if (!(bool)sendTask?.IsCompleted)       //等待当前发送任务完成
                await sendTask;

            timer.Stop();   //添加通道后自动发送获取基本信息指令，需要暂停timer轮询
            if (Send(channel.ReadBaseInfoBytes))
            {
                try
                {
                    byte[] data = await SerialPortInstance.GetResponseBytes();
                    channel.ResolveData(data, ResolveType.BaseInfoData);
                    timer.Start();
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

        private void InitializeTimer()
        {
            timer = new System.Timers.Timer();
            timer.Interval = totalMilliSeconds;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void SetTimerInterval()
        {
            if (addedAddrs.Count == 0) return;
            timer.Interval = totalMilliSeconds / addedAddrs.Count;
        }
    }
}
