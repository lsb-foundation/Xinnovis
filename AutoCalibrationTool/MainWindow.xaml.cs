using AutoCalibrationTool.Extensions;
using AutoCalibrationTool.Models;
using AutoCalibrationTool.ViewModel;
using CommonLib.Extensions;
using CommonLib.Utils;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutoCalibrationTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DeviceDataCollection _incubeCollection;
        private readonly DeviceDataCollection _roomCollection;
        private readonly BlockingCollection<string> _originDatas;
        private readonly BlockingCollection<string> _dataLines;

        public MainWindow()
        {
            InitializeComponent();
            _originDatas = new BlockingCollection<string>(5);
            _dataLines = new BlockingCollection<string>(5);
            _incubeCollection = new DeviceDataCollection();
            _roomCollection = new DeviceDataCollection();
            ViewModelLocator.Port.OnDataReceived += Port_OnDataReceived;
            Task.Run(() => GetDataLinesFromOrigin());   //后台处理收到的字符串序列
            Task.Run(() => ResolveData());              //后台解析处理过的字符串序列
        }

        private void Port_OnDataReceived(byte[] obj)
        {
            string result = Encoding.Default.GetString(obj);
            _originDatas.Add(result);
        }

        private readonly string[] _seperator = new string[] { "\r\n" };
        private void GetDataLinesFromOrigin()
        {
            var tail = string.Empty;
            foreach (var data in _originDatas.GetConsumingEnumerable())
            {
                tail += data;
                var lines = tail.Split(_seperator, StringSplitOptions.None);
                if (lines.Length == 1)
                {
                    tail = lines[0];
                }
                else if (lines.Length > 1)
                {
                    for (int idx = 0; idx < lines.Length - 1; ++idx)
                    {
                        _dataLines.Add(lines[idx]);
                    }
                    tail = lines[lines.Length - 1];
                }
            }
            //_dataLines.CompleteAdding();
        }

        private void ResolveData()
        {
            foreach (string line in _dataLines.GetConsumingEnumerable())
            {
                Task.Run(() => LoggerHelper.WriteLog($"Resolved: {line}"));

                CalibrationMode currentMode = ViewModelLocator.Main.Mode;
                if (line.StartsWith("MID") || line.StartsWith("HIGH") || line.StartsWith("LOW"))
                {
                    string[] datas = line.Split(':');
                    if (datas.Length == 5)
                    {
                        TemperatureType tempType = datas[0].ToTemperatureType();
                        int address = int.Parse(datas[1]);
                        float flow = float.Parse(datas[2]);
                        Models.ValueType valueType = datas[3].ToValueType(); //T or V
                        float value = float.Parse(datas[4]);

                        if (currentMode == CalibrationMode.Incube)
                        {
                            _incubeCollection.SetValue(tempType, valueType, address, flow, value);
                            SetDataCount();
                        }
                        else if (currentMode == CalibrationMode.Room)
                        {
                            _roomCollection.SetValue(tempType, valueType, address, flow, value);
                            SetDataCount();
                        }
                    }
                }
                else
                {
                    StatusTextBox.Dispatcher.Invoke(() =>
                    {
                        if (StatusTextBox.LineCount > 5000)
                        {
                            StatusTextBox.Clear();
                        }
                        StatusTextBox.AppendText(line + Environment.NewLine);
                        StatusTextBox.LineDown();
                    });
                }
            }
        }

        private void SetDataCount()
        {
            Dispatcher.Invoke(() => ViewModelLocator.Main.SetDataCount(
                _incubeCollection.TotalDeviceCount() + _roomCollection.TotalDeviceCount(),
                _incubeCollection.TotalFlowCount() + _roomCollection.TotalFlowCount()));
        }

        private void OnDirectoryPickerClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ViewModelLocator.Storage.StorageLocation = dialog.SelectedPath;
                }
            }
        }

        private async void OnReadTestFile(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./ttt.txt");
                using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        _dataLines.Add(line);
                    }
                }
                file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./ttt.xlsx");
                Export(_roomCollection, file);
                //string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testlog.txt");
                //using (var stream = File.OpenRead(file))
                //{
                //    var reader = new StreamReader(stream);
                //    string line = null;
                //    while((line = await reader.ReadLineAsync()) != null)
                //    {
                //        int idx = line.IndexOf("Received:");
                //        if (idx == -1) continue;
                //        var bytes = line.Substring(idx + 10).HexStringToBytes();
                //        _originDatas.Add(Encoding.Default.GetString(bytes));
                //        Thread.Sleep(20);
                //    }
                //}
            });
        }

        private void Export(DeviceDataCollection collection, string file)
        {
            using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook workbook = new XSSFWorkbook();

                BuildSheet(collection.HighTemperatureDatas, workbook, "HIGH");
                BuildSheet(collection.MidTemperatureDatas, workbook, "MID");
                BuildSheet(collection.LowTemperatureDatas, workbook, "LOW");

                workbook.Write(fs);
                workbook.Close();
            }
        }

        private void BuildSheet(List<DeviceData> deviceDatas, IWorkbook workbook, string sheetName)
        {
            if (deviceDatas.Count == 0) return;

            var orderedDeviceDatas = deviceDatas.OrderBy(dd => dd.DeviceCode)
                    .Select((v, i) => (Index: i, Data: v))
                    .GroupBy(t => t.Index / 8)
                    .Select(g => g.Select(t => t.Data).ToList());

            ISheet sheet = workbook.CreateSheet(sheetName);
            ICellStyle headerStyle = workbook.HeaderStyle();
            ICellStyle basicNumericStyle = workbook.BasicNumericStyle();
            ICellStyle temperatureStyle = workbook.BasicNumericStyle(formatter: "0.0", isBold: true, fontSize: 14.0);
            ICellStyle averageStyle = workbook.BasicNumericStyle(isBold: true);
            ICellStyle flowStyle = workbook.BasicNumericStyle(formatter: "0.0", isBold: true, fontSize: 11.0);

            int row = 0;
            foreach (IEnumerable<DeviceData> datas in orderedDeviceDatas)
            {
                int column = 0;
                int flowCount = datas.Sum(dd => dd.FlowDatas.Count);
                int voltsMaxCount = datas.Max(dd => dd.FlowDatas.Max(d => d.Volts.Count));

                #region 左侧表头
                sheet.SetCellValue(row, 0, "标定环境", headerStyle);
                sheet.MergeRegion(row, row, 1, flowCount + datas.Count() - 1, string.Empty);
                sheet.SetCellValue(row + 1, 0, "设备号", headerStyle);
                int deviceCodeRow = row + 1;
                sheet.SetCellValue(row + 2, 0, "流量", headerStyle);
                sheet.SetCellValue(row + 3, 0, "温度", headerStyle);
                sheet.SetCellValue(row + 4, 0, "最大值", headerStyle);
                sheet.SetCellValue(row + 5, 0, "最小值", headerStyle);
                sheet.SetCellValue(row + 6, 0, "差值", headerStyle);
                sheet.SetCellValue(row + 7, 0, "平均值", headerStyle);
                #endregion

                for (int deviceDataIndex = 0; deviceDataIndex < datas.Count(); ++deviceDataIndex)
                {
                    ++column;
                    var deviceData = datas.ElementAt(deviceDataIndex);

                    sheet.MergeRegion(deviceCodeRow, deviceCodeRow, column, column + deviceData.FlowDatas.Count - 1, deviceData.DeviceCode, headerStyle);   //写入设备号

                    foreach (FlowTemperatureVolts flowData in deviceData.FlowDatas.OrderBy(ftd => ftd.Flow))
                    {
                        row = deviceCodeRow;

                        sheet.SetCellValue(++row, column, flowData.Flow, flowStyle);
                        sheet.SetCellValue(++row, column, flowData.Temperature, temperatureStyle);

                        #region 写入公式
                        string range = $"{sheet.GetCell(row + 5, column).Address}:{sheet.GetCell(row + 5 + flowData.Volts.Count - 1, column).Address}";

                        ICell maxCell = sheet.GetCell(++row, column);
                        maxCell.CellFormula = $"MAX({range})";
                        maxCell.CellStyle = basicNumericStyle;

                        ICell minCell = sheet.GetCell(++row, column);
                        minCell.CellFormula = $"MIN({range})";
                        minCell.CellStyle = basicNumericStyle;

                        ICell diffCell = sheet.GetCell(++row, column);
                        diffCell.CellFormula = $"{maxCell.Address}-{minCell.Address}";
                        diffCell.CellStyle = basicNumericStyle;

                        ICell averageCell = sheet.GetCell(++row, column);
                        averageCell.CellFormula = $"AVERAGE({range})";
                        averageCell.CellStyle = averageStyle;
                        #endregion

                        //写入电压
                        foreach (float volt in flowData.Volts)
                        {
                            sheet.SetCellValue(++row, column, volt, basicNumericStyle);
                        }
                        ++column;
                    }
                }

                row = deviceCodeRow + voltsMaxCount + 8;
            }
        }

        private async void OnTestExport(object sender, RoutedEventArgs e)
        {
            string path = ViewModelLocator.Storage.StorageLocation;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            List<DeviceData> deviceDatas;
            string jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_result.json");
            using (var fs = new FileStream(jsonFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (var reader = new StreamReader(fs))
                {
                    string json = await reader.ReadToEndAsync();
                    deviceDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeviceData>>(json);
                }
            }

            if (deviceDatas == null || deviceDatas.Count == 0)
            {
                return;
            }

            for (int index = 0; index < 15; ++index)
            {
                var data = new DeviceData { DeviceCode = index + 2 };
                data.FlowDatas = deviceDatas.FirstOrDefault()?.FlowDatas;
                deviceDatas.Add(data);
            }

            var fileName = $"测试数据-{DateTime.Now: yyyyMMddHHmmssfff}.xlsx";
            var file = Path.Combine(path, fileName);
            using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook workbook = new XSSFWorkbook();
                BuildSheet(deviceDatas, workbook, "TEST");
                workbook.Write(fs);
                workbook.Close();
            }
            System.Diagnostics.Process.Start("Explorer.exe", $@"/select,{file}");
        }

        private void OnExport(object sender, RoutedEventArgs e)
        {
            string path = ViewModelLocator.Storage.StorageLocation;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (_incubeCollection.IsEmpty && _roomCollection.IsEmpty)
            {
                MessageBox.Show("没有可以导出的数据。", "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!_incubeCollection.IsEmpty)
            {
                string fileName = $"恒温标定-{DateTime.Now: yyyyMMddHHmmssfff}.xlsx";
                string file = Path.Combine(path, fileName);
                Export(_incubeCollection, file);
                System.Diagnostics.Process.Start("Explorer.exe", $@"/select,{file}");
            }

            if (!_roomCollection.IsEmpty)
            {
                string fileName = $"室温标定-{DateTime.Now: yyyyMMddHHmmssfff}.xlsx";
                string file = Path.Combine(path, fileName);
                Export(_roomCollection, file);
                System.Diagnostics.Process.Start("Explorer.exe", $@"/select,{file}");
            }

            _incubeCollection.Clear();
            _roomCollection.Clear();
            SetDataCount();
        }

        private void OnStatsusTextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (sender is TextBox box && box.LineCount > 0)
                {
                    string godModeStr = box.GetLineText(box.LineCount - 1).Trim().ToLower();
                    if (godModeStr == "test()")
                    {
                        ViewModelLocator.Storage.SetTestButtonVisibility(Visibility.Visible);
                    }
                    else if (godModeStr == "leave()")
                    {
                        ViewModelLocator.Storage.SetTestButtonVisibility(Visibility.Collapsed);
                    }
                    e.Handled = true;
                }
            }
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            if (ViewModelLocator.Main.DeviceDataCount == 0 && ViewModelLocator.Main.FlowDataCount == 0)
            {
                StatusTextBox.Clear();
            }
            else if (MessageBox.Show("该操作将清空所有已存储的数据，请确保标定流程已结束且数据已导出。", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _incubeCollection.Clear();
                _roomCollection.Clear();
                StatusTextBox.Clear();
                SetDataCount();
            }
        }

        private void OnAppExiting(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_incubeCollection.IsEmpty || !_roomCollection.IsEmpty)
            {
                if (MessageBox.Show("退出程序将丢失未保存的数据，是否确定退出？", "确定退出?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void ClearStatusTextBox(object sender, RoutedEventArgs e)
        {
            StatusTextBox.Clear();
        }

        private void OnSendButtonKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if ((sender as TextBox).DataContext is MainViewModel mainVm)
                {
                    mainVm.SendCommand?.Execute(null);
                }
            }
        }
    }
}
