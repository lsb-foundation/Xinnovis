using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MFCSoftware.Utils;
using MFCSoftwareForCUP.Extensions;
using MFCSoftwareForCUP.Models;
using MFCSoftwareForCUP.ViewModels;
using Microsoft.Win32;

namespace MFCSoftwareForCUP.Views
{
    /// <summary>
    /// ChannelUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelUserControl : UserControl
    {
        private readonly ChannelViewModel _channel;
        private readonly FlowDataSaver _saver;

        public ChannelUserControl()
        {
            InitializeComponent();
            _channel = DataContext as ChannelViewModel;
            _saver = new FlowDataSaver(30);
        }

        public event Action<ChannelUserControl> ControlRemoved;
        public event Action<ChannelUserControl> ClearAccumulateFlow;

        public int Address
        {
            get => _channel.Address;
            set => _channel.Address = value;
        }

        public DeviceExtras DeviceExtras => _channel.DeviceExtras;

        public void SetFlow(FlowData flow)
        {
            _channel.AccumulateFlow = flow.AccuFlow;
            _channel.AccumulateFlowUnit = flow.AccuFlowUnit;
            _channel.CurrentFlow = flow.CurrentFlow;
            _saver.Flow = flow;
            _saver.Flow.Unit = _channel.CurrentFlowUnit;
        }

        public void SetDeviceExtras(DeviceExtras extras) => _channel.SetExtras(extras);

        public void WhenSuccess() => Dispatcher.Invoke(() => _channel.StatusColor.Color = Colors.Green);

        public void WhenTimeOut() => Dispatcher.Invoke(() => _channel.StatusColor.Color = Colors.Yellow);

        public void WhenResolveFailed() => Dispatcher.Invoke(() => _channel.StatusColor.Color = Colors.Red);

        private void ChannelCloseButtonClick(object sender, RoutedEventArgs e) => ControlRemoved?.Invoke(this);

        private void ClearButtonClick(object sender, RoutedEventArgs e) => ClearAccumulateFlow?.Invoke(this);

        private async void ExportButtonClick(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordWindow confirm = new ConfirmPasswordWindow
            {
                Owner = this.GetParentWindow() as Window
            };
            _ = confirm.ShowDialog();
            if (!confirm.PasswordConfirmed)
            {
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "Excel文件|*.xlsx;*.xls",
                Title = "导出数据"
            };
            _ = dialog.ShowDialog();
            if (string.IsNullOrEmpty(dialog.FileName))
            {
                return;
            }

            ViewModelLocator locator = FindResource("Locator") as ViewModelLocator;
            await Task.Run(async () =>
            {
                List<FlowData> datas = await SqliteHelper.QueryFlowDatasByTimeAsync(locator.Main.AppStartTime, DateTime.Now, Address);
                FlowData.ExportFlowDatas(dialog.FileName, datas);
            });
        }
    }
}
