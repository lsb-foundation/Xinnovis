﻿using CommonLib.Extensions;
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
                channel.ControlRemoved += c => ContentWrapPanel.Children.Remove(c);
                channel.ClearAccumulateFlow += OnClearAccumulateFlow;
                channel.OnExport += OnExport;
                _ = ContentWrapPanel.Children.Add(channel);
            }
        }

        private async void OnClearAccumulateFlow(ChannelUserControl channel)
        {
            await _main.Semaphore.WaitAsync();
            SerialCommand<byte[]> command = BuildClearAccumulateFlowCommand(channel.Address);
            await SendAsync(command, channel);
            _ = _main.Semaphore.Release();
        }

        private async void OnExport(ChannelUserControl channel)
        {
            await Task.Run(async () =>
            {
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    Filter = "Excel文件|*.xlsx;*.xls",
                    Title = "导出数据"
                };

                if ((bool)dialog.ShowDialog() && !string.IsNullOrEmpty(dialog.FileName))
                {
                    List<FlowData> datas = await DbStorage.QueryFlowDatasByTimeAsync(_main.AppStartTime, DateTime.Now, channel.Address);
                    ExportHistoryFlowDataToExcel(dialog.FileName, datas);
                }
            });
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
                    channel.ControlRemoved += c => ContentWrapPanel.Children.Remove(c);
                    channel.ClearAccumulateFlow += OnClearAccumulateFlow;
                    channel.OnExport += OnExport;
                    _ = ContentWrapPanel.Children.Add(channel);
                }
            }
        }

        public void ExportHistoryFlowDataToExcel(string fileName, List<FlowData> flowDatas)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("数据流量表");

                ICellStyle headerStyle = workbook.CreateCellStyle();
                headerStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;

                sheet.SetCellValue(0, 0, "采样时间");
                sheet.SetCellValue(0, 1, "瞬时流量");
                sheet.SetCellValue(0, 2, "瞬时流量单位");
                sheet.SetCellValue(0, 3, "累积流量");
                sheet.SetCellValue(0, 4, "累积流量单位");

                ICellStyle dateStyle = workbook.CreateCellStyle();
                IDataFormat format = workbook.CreateDataFormat();
                dateStyle.DataFormat = format.GetFormat("yyyy/MM/DD HH:mm:ss");

                ICellStyle currFlowStyle = workbook.CreateCellStyle();
                currFlowStyle.DataFormat = format.GetFormat("#,##0.00"); //瞬时流量保留两位小数

                ICellStyle accuFlowStyle = workbook.CreateCellStyle();
                accuFlowStyle.DataFormat = format.GetFormat("#,###0.000"); //累积流量保留三位小数

                for (int index = 0; index < flowDatas.Count; index++)
                {
                    int row = index + 1;

                    sheet.SetCellValue(row, 0, flowDatas[index].CollectTime, dateStyle);
                    sheet.SetCellValue(row, 1, flowDatas[index].CurrentFlow, currFlowStyle);
                    sheet.SetCellValue(row, 2, flowDatas[index].Unit);
                    sheet.SetCellValue(row, 3, flowDatas[index].AccuFlow, accuFlowStyle);
                    sheet.SetCellValue(row, 4, flowDatas[index].AccuFlowUnit);
                }

                sheet.AutoSizeColumns(0, 4);
                workbook.Write(stream);
            }
        }
    }
}
