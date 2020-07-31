using CommonLib.Communication.Serial;
using CommonLib.Extensions;
using MFCSoftware.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MFCSoftware
{
    /// <summary>
    /// MFCSoftwareWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MFCSoftwareWindow : Window
    {
        private ConcurrentDictionary<int, ChannelUserControl> controlMap = new ConcurrentDictionary<int, ChannelUserControl>();
        private ConcurrentQueue<byte> buffer = new ConcurrentQueue<byte>();

        private LinkedList<ChannelUserControl> controlList = new LinkedList<ChannelUserControl>();
        private LinkedListNode<ChannelUserControl> currentNode;
        private System.Timers.Timer timer;

        private AdvancedSerialPort serialPort;
        private int totalMilliSeconds = 1000;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public MFCSoftwareWindow()
        {
            InitializeComponent();
            this.Closed += MFCSoftwareWindow_Closed;
            serialPort = AppSerialPortInstance.GetSerialPortInstance();
            serialPort.ReceivedDataHandler = AddDataToBuffer;
            InitializeTimer();
            RunResolvingTask();
        }

        private void AddDataToBuffer(byte[] data)
        {
            foreach (byte item in data)
                buffer.Enqueue(item);
        }

        /// <summary>
        /// 一级数据解析
        /// </summary>
        private void RunResolvingTask()
        {
            Task.Run(() =>
            {
                byte[] bytesArray = null;
                List<byte> bytes = new List<byte>();

                while (true)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;

                    if (buffer.TryDequeue(out byte item))
                    {
                        bytes.Add(item);

                        //长度37为读取基本数据，其他数据长度为7
                        if (bytes.Count == 7 || bytes.Count == 37)
                            bytesArray = bytes.ToArray();
                        else continue;

                        if (bytesArray.CheckCRC16ByDefault())
                        {
                            int addr = bytesArray[0];
                            if (controlMap.TryGetValue(addr, out ChannelUserControl channel))
                            {
                                channel.ResolveData(bytesArray);
                                bytes.Clear();
                                bytesArray = null;
                            }
                        }
                    }
                }
            }, cancellationTokenSource.Token);
        }

        //关闭窗口，清理资源
        private void MFCSoftwareWindow_Closed(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
            if (timer.Enabled) timer.Stop();
            timer.Dispose();

            serialPort.ReceivedDataHandler = null;
            ChannelGrid.Children.Clear();

            controlMap.Clear();
            controlMap = null;

            controlList.Clear();
            controlList = null;
            currentNode = null;
            buffer = null;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (controlList.Count == 0) return;

            if (currentNode == null || currentNode == controlList.Last)
                currentNode = controlList.First;
            else currentNode = currentNode.Next;

            if(currentNode?.Value != null)
            {
                var channel = currentNode.Value;
                if (Send(channel.ReadFlowBytes))
                    channel.DataSended?.Invoke();
            }
        }

        private bool Send(byte[] data)
        {
            try
            {
                if (!serialPort.IsOpen)
                    serialPort.Open();
                serialPort.Write(data, 0, data.Length);
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
                if (!controlMap.Keys.Contains(window.Address))
                {
                    ChannelUserControl channelControl = new ChannelUserControl();
                    channelControl.SetAddress(window.Address);
                    channelControl.ControlWasRemoved += Channel_ControlWasRemoved;
                    ChannelGrid.Children.Add(channelControl);
                    SetTimerInterval();
                    controlMap.TryAdd(window.Address, channelControl);
                    controlList.AddLast(channelControl);
                    ChannelAdded(channelControl);
                }
                else MessageBox.Show("地址重复。");
            }
        }

        private async void ChannelAdded(ChannelUserControl channel)
        {   //添加通道后自动发送获取基本信息指令，需要暂停timer轮询
            timer.Stop();
            if (Send(channel.ReadBaseInfoBytes))
            {
                channel.DataSended?.Invoke();
                await Task.Delay(100);
                timer.Start();
            }
        }

        private void Channel_ControlWasRemoved(object sender)
        {
            timer.Stop();
            var control = sender as ChannelUserControl;
            controlMap.TryRemove(control.Address, out ChannelUserControl _);
            controlList.Remove(control);
            SetTimerInterval();
            ChannelGrid.Children.Remove(control);
            timer.Start();
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
            if (controlMap.Count == 0) return;
            timer.Interval = totalMilliSeconds / controlMap.Count;
        }
    }
}
