using CommonLib.Extensions;
using MFCSoftware.Utils;
using CommonLib.Utils;
using MFCSoftwareForCUP.Models;
using MFCSoftwareForCUP.ViewModels;
using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;

namespace MFCSoftwareForCUP.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _main;
        private readonly byte[] _clearFlowBytes = new byte[] { 0x06, 0x00, 0x18, 0x00, 0x00 };
        private readonly byte[] _readFlowBytes = new byte[] { 0x03, 0x00, 0x16, 0x00, 0x0B };
        private readonly CancellationTokenSource _cancel = new();

        public MainWindow()
        {
            InitializeComponent();
            _main = DataContext as MainViewModel;
        }

        private void AddChannelButtonClick(object sender, RoutedEventArgs e)
        {
            if (_main.AddressToAdd > 0
                && _main.AddressToAdd <= _main.MaxDeviceCount
                && ContentWrapPanel.Children.OfType<ChannelUserControl>().All(uc => uc.Address != _main.AddressToAdd))
            {
                ChannelUserControl channel = new()
                {
                    Address = _main.AddressToAdd
                };
                channel.ControlRemoved += OnControlRemove;
                channel.ClearAccumulateFlow += OnClearAccumulateFlow;
                _ = ContentWrapPanel.Children.Add(channel);
            }
        }

        private void OnControlRemove(ChannelUserControl channel)
        {
            if (ConfirmPassword())
            {
                ContentWrapPanel.Children.Remove(channel);
            }
        }

        private async void OnClearAccumulateFlow(ChannelUserControl channel)
        {
            if (!ConfirmPassword())
            {
                return;
            }
            await _main.Semaphore.WaitAsync();
            SerialCommand<byte[]> command = BuildClearAccumulateFlowCommand(channel.Address);
            await SendAsync(command, channel);
            _ = _main.Semaphore.Release();
        }

        private SerialCommand<byte[]> BuildReadFlowCommand(int address)
        {
            return new SerialCommandBuilder(SerialCommandType.ReadFlow)
                .AppendAddress(address)
                .AppendBytes(_readFlowBytes)
                .AppendCrc16()
                .WithResponseLength(27)
                .Build();
        }

        private SerialCommand<byte[]> BuildClearAccumulateFlowCommand(int address)
        {
            return new SerialCommandBuilder(SerialCommandType.ClearAccuFlowData)
                .AppendAddress(address)
                .AppendBytes(_clearFlowBytes)
                .AppendCrc16()
                .WithResponseLength(7)
                .Build();
        }

        private void StartLoopToSend()
        {
            Task.Factory.StartNew(async () =>
            {
                int address = 1;
                while (!_cancel.IsCancellationRequested)
                {
                    LoggerHelper.WriteLog("轮询地址： " + address);

                    if (address <= _main.MaxDeviceCount)
                    {
                        await _main.Semaphore.WaitAsync(_cancel.Token);

                        var command = BuildReadFlowCommand(address);

                        var channel = await Dispatcher.InvokeAsync(
                            callback: () => ContentWrapPanel.Children.OfType<ChannelUserControl>().FirstOrDefault(uc => uc.Address == address),
                            priority: DispatcherPriority.Normal,
                            cancellationToken: _cancel.Token);

                        await SendAsync(command, channel);
                        _ = _main.Semaphore.Release();
                    }

                    address = address < _main.MaxDeviceCount ? address + 1 : 1;
                    Thread.Sleep(5);
                }
            }, _cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
            
        private async Task SendAsync(SerialCommand<byte[]> command, ChannelUserControl channel)
        {
            try
            {
                LoggerHelper.WriteLog($"Send: {command}");
                var data = await SerialPortInstance.GetResponseAsync(command);
                LoggerHelper.WriteLog($"Received: {data.ToHexString()}");

                if (!data.CheckCRC16ByDefault())
                {
                    throw new Exception("CRC校验失败。");
                }

                if (channel != null && command.Type == SerialCommandType.ReadFlow)
                {
                    var flow = FlowData.ResolveFromBytes(data);
                    flow.Address = channel.Address;
                    channel.SetFlow(flow);
                    channel.WhenSuccess();
                }
            }
            catch (TimeoutException)
            {
                channel?.WhenTimeOut();
            }
            catch (Exception e)
            {
                LoggerHelper.WriteLog(e.Message, e);
                channel?.WhenResolveFailed();
            }
        }

        private async void AppClosing(object sender, CancelEventArgs e)
        {
            _cancel.Cancel();
            AppJsonModel model = new()
            {
                PortName = _main.PortName,
                DeviceMaxCount = _main.MaxDeviceCount,
                AddressToAdd = _main.AddressToAdd,
                Devices = new List<DeviceExtras>()
            };
            foreach (ChannelUserControl uc in ContentWrapPanel.Children)
            {
                model.Devices.Add(uc.DeviceExtras);
            }
            await model.SaveAsync();
        }

        private async void AppLoaded(object sender, RoutedEventArgs e)
        {
            if (await AppJsonModel.LoadAsync() is AppJsonModel model)
            {
                _main.PortName = model.PortName;
                _main.MaxDeviceCount = model.DeviceMaxCount;
                _main.AddressToAdd = model.AddressToAdd;
                foreach (DeviceExtras extras in model.Devices)
                {
                    ChannelUserControl channel = new()
                    {
                        Address = extras.Address
                    };
                    channel.SetDeviceExtras(extras);
                    channel.ControlRemoved += OnControlRemove;
                    channel.ClearAccumulateFlow += OnClearAccumulateFlow;
                    _ = ContentWrapPanel.Children.Add(channel);
                }
            }
            StartLoopToSend();
        }

        private void ResetPasswordButtonClick(object sender, RoutedEventArgs e)
        {
            ResetPasswordWindow reset = new() { Owner = this };
            _ = reset.ShowDialog();
        }

        private async void ExportSummaryButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ConfirmPassword())
            {
                return;
            }

            SaveFileDialog dialog = new()
            {
                Filter = "Excel文件|*.xlsx;*.xls",
                Title = "导出数据"
            };
            _ = dialog.ShowDialog();
            if (string.IsNullOrEmpty(dialog.FileName))
            {
                return;
            }

            string file = dialog.FileName;
            List<DeviceExtras> extras = new();
            foreach (ChannelUserControl channel in ContentWrapPanel.Children)
            {
                extras.Add(channel.DeviceExtras);
            }
            await Task.Run(() => ExportSummary(extras, file));
        }

        private async void ExportSummary(List<DeviceExtras> extras, string file)
        {
            List<FlowData> flows = await SqliteHelper.QueryLatestAccumulateFlowDatasAsync();
            using FileStream stream = new(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("流量汇总");

            ICellStyle headerStyle = workbook.HeaderStyle();
            sheet.SetCellValue(0, 0, "楼层号", headerStyle);
            sheet.SetCellValue(0, 1, "房间号", headerStyle);
            sheet.SetCellValue(0, 2, "气体类型", headerStyle);
            sheet.SetCellValue(0, 3, "累积流量", headerStyle);
            sheet.SetCellValue(0, 4, "单位", headerStyle);
            sheet.SetCellValue(0, 5, "采集时间", headerStyle);

            ICellStyle basicStyle = workbook.BasicStyle();
            ICellStyle dateStyle = workbook.FormattedStyle("yyyy/MM/dd HH:mm:ss");

            DeviceExtras[] orderExtras = extras.OrderBy(e => e.Floor).ThenBy(e => e.Room).ToArray();
            for (int index = 0; index < orderExtras.Length; ++index)
            {
                DeviceExtras extra = orderExtras[index];
                if (flows.FirstOrDefault(f => f.Address == extra.Address) is FlowData flow)
                {
                    int row = index + 1;
                    sheet.SetCellValue(row, 0, extra.Floor, basicStyle);
                    sheet.SetCellValue(row, 1, extra.Room, basicStyle);
                    sheet.SetCellValue(row, 2, extra.GasType, basicStyle);
                    sheet.SetCellValue(row, 3, flow.AccuFlow, basicStyle);
                    sheet.SetCellValue(row, 4, flow.AccuFlowUnit, basicStyle);
                    sheet.SetCellValue(row, 5, flow.CollectTime, dateStyle);
                }
            }
            sheet.AutoSizeColumns(0, 5);
            workbook.Write(stream);
            workbook.Close();
        }

        private bool ConfirmPassword()
        {
            ConfirmPasswordWindow confirm = new() { Owner = this };
            _ = confirm.ShowDialog();
            return confirm.PasswordConfirmed;
        }
    }
}
