using System;
using System.Windows.Controls;
using System.Windows.Media;
using CommonLib.MfcUtils;
using MFCSoftwareForCUP.Models;
using MFCSoftwareForCUP.ViewModels;

namespace MFCSoftwareForCUP.Views
{
    /// <summary>
    /// ChannelUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelUserControl : UserControl
    {
        private readonly ChannelViewModel _channel;
        private FlowDataSaver saver;

        public ChannelUserControl()
        {
            InitializeComponent();
            _channel = DataContext as ChannelViewModel;
        }

        public event Action<ChannelUserControl> ControlRemoved;
        public event Action<ChannelUserControl> ClearAccumulateFlow;
        public event Action<ChannelUserControl> OnExport;

        public int Address
        {
            get => _channel.Address;
            set
            {
                _channel.Address = value;
                if (saver == null)
                {
                    saver = new FlowDataSaver(30);
                }
            }
        }

        public DeviceExtras DeviceExtras => _channel.DeviceExtras;

        public void SetFlow(FlowData flow)
        {
            _channel.AccumulateFlow = flow.AccuFlow;
            _channel.AccumulateFlowUnit = flow.AccuFlowUnit;
            _channel.CurrentFlow = flow.CurrentFlow;
            _channel.CurrentFlowUnit = flow.Unit;
            saver.Flow = flow;
        }

        public void SetDeviceExtras(DeviceExtras extras) => _channel.SetExtras(extras);
        public void WhenSuccess() => Dispatcher.Invoke(() => _channel.StatusColor.Color = Colors.Green);
        public void WhenTimeOut() => Dispatcher.Invoke(() => _channel.StatusColor.Color = Colors.Yellow);
        public void WhenResolveFailed() => Dispatcher.Invoke(() => _channel.StatusColor.Color = Colors.Red);

        private void ChannelCloseButtonClick(object sender, System.Windows.RoutedEventArgs e) => ControlRemoved?.Invoke(this);

        private void ClearButtonClick(object sender, System.Windows.RoutedEventArgs e) => ClearAccumulateFlow?.Invoke(this);

        private void ExportButtonClick(object sender, System.Windows.RoutedEventArgs e) => OnExport?.Invoke(this);
    }
}
