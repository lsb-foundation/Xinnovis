using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommonLib.Extensions;
using CommonLib.MfcUtils;
using MFCSoftwareForCUP.Extensions;
using MFCSoftwareForCUP.Models;
using MFCSoftwareForCUP.ViewModels;
using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

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
            _channel.CurrentFlowUnit = flow.Unit;
            _saver.Flow = flow;
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
                List<FlowData> datas = await DbStorage.QueryFlowDatasByTimeAsync(locator.Main.AppStartTime, DateTime.Now, Address);
                ExportHistory(dialog.FileName, datas);
            });
        }

        private void ExportHistory(string fileName, List<FlowData> flowDatas)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("数据流量表");

                ICellStyle headerStyle = workbook.HeaderStyle();
                sheet.SetCellValue(0, 0, "采样时间", headerStyle);
                sheet.SetCellValue(0, 1, "瞬时流量", headerStyle);
                sheet.SetCellValue(0, 2, "瞬时流量单位", headerStyle);
                sheet.SetCellValue(0, 3, "累积流量", headerStyle);
                sheet.SetCellValue(0, 4, "累积流量单位", headerStyle);

                ICellStyle basicStyle = workbook.BasicStyle();
                ICellStyle dateStyle = workbook.FormattedStyle("yyyy/MM/DD HH:mm:ss");
                ICellStyle currFlowStyle = workbook.FormattedStyle("#,##0.00");
                ICellStyle accuFlowStyle = workbook.FormattedStyle("#,###0.000");

                for (int index = 0; index < flowDatas.Count; index++)
                {
                    int row = index + 1;
                    FlowData flow = flowDatas[index];
                    sheet.SetCellValue(row, 0, flow.CollectTime, dateStyle);
                    sheet.SetCellValue(row, 1, flow.CurrentFlow, currFlowStyle);
                    sheet.SetCellValue(row, 2, flow.Unit, basicStyle);
                    sheet.SetCellValue(row, 3, flow.AccuFlow, accuFlowStyle);
                    sheet.SetCellValue(row, 4, flow.AccuFlowUnit, basicStyle);
                }

                sheet.AutoSizeColumns(0, 4);
                workbook.Write(stream);
                workbook.Close();
            }
        }
    }
}
