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
using System.Windows.Forms;

namespace AutoCalibrationTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<DeviceData> _incubeDeviceDatas;
        private readonly List<DeviceData> _roomDeviceDatas;
        private readonly BlockingCollection<string> _originDatas;
        private readonly BlockingCollection<string> _dataLines;

        public MainWindow()
        {
            InitializeComponent();
            _originDatas = new BlockingCollection<string>(5);
            _dataLines = new BlockingCollection<string>(4);
            _incubeDeviceDatas = new List<DeviceData>();
            _roomDeviceDatas = new List<DeviceData>();
            ViewModelLocator.Port.OnDataReceived += Port_OnDataReceived;
            Task.Run(() => GetDataLinesFromOrigin());   //后台处理收到的字符串序列
            Task.Run(() => ResolveData());              //后台解析处理过的字符串序列
        }

        private void Port_OnDataReceived(byte[] obj)
        {
            string result = Encoding.Default.GetString(obj);
            _originDatas.Add(result);
        }

        private void GetDataLinesFromOrigin()
        {
            var tail = string.Empty;
            var separator = new string[] { "\r\n" };
            foreach (var data in _originDatas.GetConsumingEnumerable())
            {
                var lines = data.Split(separator, StringSplitOptions.None);
                if (lines.Length == 1)
                {
                    tail += lines[0];
                }
                else if (lines.Length > 1)
                {
                    _dataLines.Add(tail + lines[0]);
                    for (int idx = 1; idx < lines.Length - 1; ++idx)
                    {
                        _dataLines.Add(lines[idx]);
                    }

                    tail = lines[lines.Length - 1];
                }
            }
            _dataLines.CompleteAdding();
        }

        private void ResolveData()
        {
            foreach (string line in _dataLines.GetConsumingEnumerable())
            {
                Task.Run(() => LoggerHelper.WriteLog($"Resolved: {line}"));
                if (line.StartsWith("MID") || line.StartsWith("HIGH") || line.StartsWith("LOW"))
                {
                    string[] datas = line.Split(':');
                    if (datas.Length == 5)
                    {
                        int address = int.Parse(datas[1]);
                        int flow = int.Parse(datas[2]);
                        string flag = datas[3];
                        float value = float.Parse(datas[4]);

                        CalibrationMode currentMode = ViewModelLocator.Main.Mode;
                        if (currentMode == CalibrationMode.Incube)
                        {
                            AddDeviceDataTo(_incubeDeviceDatas, address, flow, value, flag);
                        }
                        else if (currentMode == CalibrationMode.Room)
                        {
                            AddDeviceDataTo(_roomDeviceDatas, address, flow, value, flag);
                        }
                    }
                }
                else
                {
                    StatusTextBox.Dispatcher.Invoke(() =>
                    {
                        StatusTextBox.AppendText(line + Environment.NewLine);
                        StatusTextBox.LineDown();
                    });
#if DEBUG
                    if (line.Contains("标定结束"))
                    {
                        string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"test_incube_result-{DateTime.Now.ToString("yyyyMMddHHmmss")}.json");
                        using (FileStream stream = File.Create(file))
                        {
                            using (var writer = new StreamWriter(stream))
                            {
                                var json = Newtonsoft.Json.JsonConvert.SerializeObject(_incubeDeviceDatas);
                                writer.Write(json);
                            }
                        }
                        file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"test_room_result-{DateTime.Now: yyyyMMddHHmmss}.json");
                        using (FileStream stream = File.Create(file))
                        {
                            using (var writer = new StreamWriter(stream))
                            {
                                var json = Newtonsoft.Json.JsonConvert.SerializeObject(_roomDeviceDatas);
                                writer.Write(json);
                            } 
                        }
                    }
#endif
                }
            }
        }

        private void AddDeviceDataTo(List<DeviceData> deviceDatas, int address, int flow, float value, string flag)
        {
            if (deviceDatas.Any(dd => dd.DeviceCode == address))
            {
                DeviceData deviceData = deviceDatas.FirstOrDefault(dd => dd.DeviceCode == address);
                if (deviceData.Datas.Any(d => d.Flow == flow))
                {
                    FlowTemperatureData tf = deviceData.Datas.FirstOrDefault(d => d.Flow == flow);
                    if (flag == "T")
                    {
                        tf.Temperature = value;
                    }
                    else if (flag == "V")
                    {
                        tf.Volts.Add(value);
                    }
                }
                else
                {
                    var tf = new FlowTemperatureData { Flow = flow, Volts = new List<float>() };
                    if (flag == "T")
                    {
                        tf.Temperature = value;
                    }
                    else if (flag == "V")
                    {
                        tf.Volts.Add(value);
                    }
                    deviceData.Datas.Add(tf);
                }
            }
            else
            {
                var ft = new FlowTemperatureData { Flow = flow, Volts = new List<float>() };
                if (flag == "T")
                {
                    ft.Temperature = value;
                }
                else if (flag == "V")
                {
                    ft.Volts.Add(value);
                }
                DeviceData deviceData = new DeviceData { DeviceCode = address, Datas = new List<FlowTemperatureData>() };
                deviceData.Datas.Add(ft);
                deviceDatas.Add(deviceData);
            }
        }

        private void OnDirectoryPickerClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
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
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.txt");
                using (FileStream stream = File.OpenRead(file))
                {
                    var reader = new StreamReader(stream);
                    string line = null;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        _originDatas.Add(line + "\r\n");
                        Thread.Sleep(20);
                    }
                }
                _originDatas.CompleteAdding();
            });
        }

        private void Export(List<DeviceData> deviceDatas, string file)
        {
            var orderedDeviceDatas = deviceDatas.OrderBy(dd => dd.DeviceCode)
                    .Select((v, i) => (Index: i, Data: v))
                    .GroupBy(t => t.Index / 8)
                    .Select(g => g.Select(t => t.Data).ToList())
                    .ToList();

            using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet();

                ICellStyle basicStyle = workbook.BasicStyle();
                ICellStyle headerStyle = workbook.HeaderStyle();
                ICellStyle basicNumericStyle = workbook.BasicNumericStyle();
                ICellStyle temperatureStyle = workbook.TemperatureCellStyle();
                ICellStyle averageStyle = workbook.AverageCellStyle();

                int row = 0;
                for (int line = 0; line < orderedDeviceDatas.Count; ++line)
                {
                    int column = 0;
                    List<DeviceData> datas = orderedDeviceDatas[line];
                    int flowCount = datas.Sum(dd => dd.Datas.Count);
                    int voltsMaxCount = datas.Max(dd => dd.Datas.Max(d => d.Volts.Count));

#region 左侧表头
                    sheet.SetCellValue(row, 0, "标定环境", headerStyle);
                    sheet.MergeRegion(row, row, 1, flowCount + datas.Count - 1, string.Empty);
                    sheet.SetCellValue(row + 1, 0, "设备号", headerStyle);
                    int deviceCodeRow = row + 1;
                    sheet.SetCellValue(row + 2, 0, "流量", headerStyle);
                    sheet.SetCellValue(row + 3, 0, "温度", headerStyle);
                    sheet.SetCellValue(row + 4, 0, "最大值", headerStyle);
                    sheet.SetCellValue(row + 5, 0, "最小值", headerStyle);
                    sheet.SetCellValue(row + 6, 0, "差值", headerStyle);
                    sheet.SetCellValue(row + 7, 0, "平均值", headerStyle);
#endregion

                    for (int deviceDataIndex = 0; deviceDataIndex < datas.Count; ++deviceDataIndex)
                    {
                        ++column;
                        var deviceData = datas.ElementAt(deviceDataIndex);
#region 写入设备号
                        sheet.MergeRegion(deviceCodeRow, deviceCodeRow, column, column + deviceData.Datas.Count - 1, deviceData.DeviceCode, headerStyle);
#endregion

                        for (int dataIndex = 0; dataIndex < deviceData.Datas.Count; ++dataIndex)
                        {
                            var data = deviceData.Datas[dataIndex];
                            row = deviceCodeRow;

                            sheet.SetCellValue(++row, column, data.Flow, headerStyle);
                            sheet.SetCellValue(++row, column, data.Temperature, temperatureStyle);

                            string range = $"{sheet.GetCell(row + 5, column).Address}:{sheet.GetCell(row + 5 + data.Volts.Count - 1, column).Address}";

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

                            foreach (float volt in data.Volts)
                            {
                                sheet.SetCellValue(++row, column, volt, basicNumericStyle);
                            }
                            ++column;
                        }
                    }

                    row = deviceCodeRow + voltsMaxCount + 7;
                }
                workbook.Write(fs);
                workbook.Close();
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
                DeviceData data = deviceDatas[0];
                data.DeviceCode = index + 1;
                deviceDatas.Add(data);
            }

            var fileName = $"Export_{DateTime.Now: yyyyMMddHHmmss}.xlsx";
            var file = Path.Combine(path, fileName);
            Export(deviceDatas, file);
        }

        private void OnExport(object sender, RoutedEventArgs e)
        {
            string path = ViewModelLocator.Storage.StorageLocation;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (_incubeDeviceDatas.Count > 0)
            {
                string fileName = $"恒温标定-{DateTime.Now: yyyyMMddHHmmssfff}.xlsx";
                string file = Path.Combine(path, fileName);
                Export(_incubeDeviceDatas, file);
                System.Diagnostics.Process.Start("Explorer.exe", $@"/select,{file}");
            }

            if (_roomDeviceDatas.Count > 0)
            {
                string fileName = $"室温标定-{DateTime.Now: yyyyMMddHHmmssfff}.xlsx";
                string file = Path.Combine(path, fileName);
                Export(_roomDeviceDatas, file);
                System.Diagnostics.Process.Start("Explorer.exe", $@"/select,{file}");
            }

            _incubeDeviceDatas.Clear();
            _roomDeviceDatas.Clear();
        }

        private void OnStatsusTextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (sender is System.Windows.Controls.TextBox box)
                {
                    string godModeStr = box.GetLineText(box.LineCount - 1);
                    if (godModeStr.Trim().ToLower() == "test()")
                    {
                        ViewModelLocator.Storage.SetTestButtonToVisible();
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
