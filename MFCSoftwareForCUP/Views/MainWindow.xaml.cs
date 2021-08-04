using CommonLib.Extensions;
using CommonLib.MfcUtils;
using CommonLib.Models;
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
        private readonly List<UnitCode> _unitCodes = UnitCode.GetUnitCodesFromConfiguration();
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
            _main = DataContext as MainViewModel;
            _ = Task.Run(() => LoopToSend());
        }

        private void AddChannelButtonClick(object sender, RoutedEventArgs e)
        {
            if (_main.AddressToAdd > 0
                && _main.AddressToAdd <= _main.MaxDeviceCount
                && ContentWrapPanel.Children.OfType<ChannelUserControl>().All(uc => uc.Address != _main.AddressToAdd))
            {
                ChannelUserControl channel = new ChannelUserControl
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
                .ToSerialCommand(27);
        }

        private SerialCommand<byte[]> BuildClearAccumulateFlowCommand(int address)
        {
            return new SerialCommandBuilder(SerialCommandType.ClearAccuFlowData)
                .AppendAddress(address)
                .AppendBytes(_clearFlowBytes)
                .AppendCrc16()
                .ToSerialCommand(7);
        }

        private async void LoopToSend()
        {
            int address = 1;
            while (true)
            {
                if (_tokenSource.IsCancellationRequested)
                {
                    return;
                }

                if (address <= _main.MaxDeviceCount)
                {
                    await _main.Semaphore.WaitAsync();
                    SerialCommand<byte[]> command = BuildReadFlowCommand(address);
                    ChannelUserControl channel = await Dispatcher.InvokeAsync(() => ContentWrapPanel.Children.OfType<ChannelUserControl>().FirstOrDefault(uc => uc.Address == address));
                    await SendAsync(command, channel);
                    address = address < _main.MaxDeviceCount ? address + 1 : 1;
                    _ = _main.Semaphore.Release();
                }

                Thread.Sleep(5);
            }
        }

        private async Task SendAsync(SerialCommand<byte[]> command, ChannelUserControl channel)
        {
            try
            {
                await SerialPortInstance.SendAsync(command);
                LoggerHelper.WriteLog($"Send: {command}");

                byte[] data = await SerialPortInstance.GetResponseBytesAsync();
                LoggerHelper.WriteLog($"Received: {data.ToHexString()}");

                if (!data.CheckCRC16ByDefault())
                {
                    throw new Exception("CRC校验失败。");
                }

                if (command.Type == SerialCommandType.ReadFlow)
                {
                    ResolveFlowData(data, channel);
                }
                else if (command.Type == SerialCommandType.ClearAccuFlowData)
                {
                    _ = ResolveActionAttribute.CheckAutomatically(data, command.Type);  //测试解析，不进行其他操作
                }

                channel?.WhenSuccess();
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

        private void ResolveFlowData(byte[] data, ChannelUserControl channel)
        {
            Span<byte> dataSpan = data.AsSpan();
            float flow = dataSpan.Slice(3, 4).ToInt32ForHighFirst() / 100.0f;
            Span<byte> accuFlowSpan = dataSpan.Slice(7, 8);
            accuFlowSpan.Reverse();
            float accuFlow = BitConverter.ToInt64(accuFlowSpan.ToArray(), 0) / 1000.0f;
            int unitCode = dataSpan.Slice(15, 2).ToInt32ForHighFirst();

            string unit = string.Empty;
            if (unitCode == 0)
            {
                unit = "L";
            }
            else if (unitCode == 1)
            {
                unit = "m³";
            }

            int days = dataSpan.Slice(17, 2).ToInt32ForHighFirst();
            int hours = dataSpan.Slice(19, 2).ToInt32ForHighFirst();
            int mins = dataSpan.Slice(21, 2).ToInt32ForHighFirst();
            int secs = dataSpan.Slice(23, 2).ToInt32ForHighFirst();

            FlowData flowData = new FlowData
            {
                Address = channel.Address,
                CurrentFlow = flow,
                Unit = _unitCodes?.FirstOrDefault(u => u.Code == unitCode)?.Unit,
                AccuFlow = accuFlow,
                AccuFlowUnit = unit,
                Days = days,
                Hours = hours,
                Minutes = mins,
                Seconds = secs
            };
            channel?.SetFlow(flowData);
        }

        private void AppClosed(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
            AppJsonModel model = new AppJsonModel
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
            model.Save();
        }

        private async void AppLoaded(object sender, RoutedEventArgs e)
        {
            if (await AppJsonModel.ReadFromFileAsync() is AppJsonModel model)
            {
                _main.PortName = model.PortName;
                _main.MaxDeviceCount = model.DeviceMaxCount;
                _main.AddressToAdd = model.AddressToAdd;
                foreach (DeviceExtras extras in model.Devices)
                {
                    ChannelUserControl channel = new ChannelUserControl
                    {
                        Address = extras.Address
                    };
                    channel.SetDeviceExtras(extras);
                    channel.ControlRemoved += OnControlRemove;
                    channel.ClearAccumulateFlow += OnClearAccumulateFlow;
                    _ = ContentWrapPanel.Children.Add(channel);
                }
            }
        }

        private void ResetPasswordButtonClick(object sender, RoutedEventArgs e)
        {
            ResetPasswordWindow reset = new ResetPasswordWindow { Owner = this };
            _ = reset.ShowDialog();
        }

        private async void ExportSummaryButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ConfirmPassword())
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

            string file = dialog.FileName;
            List<DeviceExtras> extras = new List<DeviceExtras>();
            foreach (ChannelUserControl channel in ContentWrapPanel.Children)
            {
                extras.Add(channel.DeviceExtras);
            }
            await Task.Run(() => ExportSummary(extras, file));
        }

        private async void ExportSummary(List<DeviceExtras> extras, string file)
        {
            List<FlowData> flows = await DbStorage.QueryLatestAccumulateFlowDatasAsync();
            using (FileStream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
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
        }

        private bool ConfirmPassword()
        {
            ConfirmPasswordWindow confirm = new ConfirmPasswordWindow { Owner = this };
            _ = confirm.ShowDialog();
            return confirm.PasswordConfirmed;
        }
    }
}
