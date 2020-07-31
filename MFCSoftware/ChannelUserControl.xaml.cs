using MFCSoftware.ViewModels;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MFCSoftware
{
    /// <summary>
    /// ChannelUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelUserControl : UserControl
    {
        private ChannelUserControlViewModel viewModel = new ChannelUserControlViewModel();
        private System.Timers.Timer timer = new System.Timers.Timer(500);
        private bool received;
        
        public ChannelUserControl()
        {
            InitializeComponent();
            this.Loaded += Control_Loaded;
            viewModel.AppendPoint = point => viewModel.PointDataSource.AppendAsync(this.Dispatcher, point);
            this.DataContext = viewModel;

            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            DataSended = () =>
            {
                received = false;
                timer.Start();
            };
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            if (!received)
            {
                Console.WriteLine("接收超时");
            }
        }

        public event Action<object> ControlWasRemoved; //控件被移除
        public Action DataSended { get; }

        public int Address { get => viewModel.Address; }
        public byte[] ReadFlowBytes { get => viewModel.ReadFlowBytes; }
        public byte[] ReadBaseInfoBytes { get => viewModel.ReadBaseInfoBytes; }

        private void Closed(object sender, RoutedEventArgs e)
        {
            ControlWasRemoved?.Invoke(this);
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            plotter.AddLineGraph(viewModel.PointDataSource, Colors.Green, 2, "瞬时流量");
            plotter.Viewport.FitToView();
        }

        /// <summary>
        /// 二级数据解析
        /// </summary>
        /// <param name="data"></param>
        public void ResolveData(byte[] data)
        {
            received = true;
            viewModel.SencondResolve(data);
        }

        public void SetAddress(int addr) => viewModel.SetAddress(addr);
    }
}
