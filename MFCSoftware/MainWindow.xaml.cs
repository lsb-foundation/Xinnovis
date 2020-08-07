using MaterialDesignThemes.Wpf;
using MFCSoftware.Common;
using MFCSoftware.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MFCSoftware
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<int> addedAddrs = new List<int>();
        private LinkedList<ChannelUserControl> controlList = new LinkedList<ChannelUserControl>();
        private LinkedListNode<ChannelUserControl> currentNode;
        private System.Timers.Timer timer;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private int totalMilliSeconds = 1000;
        
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            InitializeTimer();
        }

        private void OpenSetSerialPortWindow(object sender, RoutedEventArgs e)
        {
            SetSerialPortWindow window = new SetSerialPortWindow();
            window.Owner = this;
            window.ShowInTaskbar = false;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }

        private void OpenSetAddressWindow(object sender, RoutedEventArgs e)
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
            cancellationTokenSource.Cancel();

            timer.Stop();
            timer.Dispose();

            ChannelGrid.Children.Clear();
            addedAddrs.Clear();
            controlList.Clear();
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
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
                        byte[] data = await AppSerialPortInstance.GetResponseBytes();
                        channel.ResolveData(data, ResolveType.FlowData);
                    }
                    catch (TimeoutException)
                    {
                        channel.WhenTimeOut();
                    }
                    catch { }
                }
            }
        }

        private bool Send(SerialCommand<byte[]> sc)
        {
            try
            {
                AppSerialPortInstance.Send(sc);
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
                }
                else MessageBox.Show("地址重复。");
            }
        }

        private async void ChannelControl_ClearAccuFlowClicked(ChannelUserControl channel)
        {
            timer.Stop();
            if (Send(channel.ClearAccuFlowBytes))
            {
                try
                {
                    byte[] data = await AppSerialPortInstance.GetResponseBytes();
                    channel.ResolveData(data, ResolveType.ClearAccuFlowData);
                    timer.Start();
                }
                catch (TimeoutException) { }
                catch { }
            }
        }

        private void ChannelControl_ControlWasRemoved(ChannelUserControl sender)
        {
            timer.Stop();
            addedAddrs.Remove(sender.Address);
            controlList.Remove(sender);
            ChannelGrid.Children.Remove(sender);
            SetTimerInterval();
            timer.Start();
        }

        private async void ChannelAdded(ChannelUserControl channel)
        {
            timer.Stop();   //添加通道后自动发送获取基本信息指令，需要暂停timer轮询
            if (Send(channel.ReadBaseInfoBytes))
            {
                try
                {
                    byte[] data = await AppSerialPortInstance.GetResponseBytes();
                    channel.ResolveData(data, ResolveType.BaseInfoData);
                    timer.Start();
                }
                catch (TimeoutException) { }
                catch { }
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
