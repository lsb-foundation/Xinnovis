using AutoCommander.ViewModels;
using CommonLib.Extensions;
using Microsoft.Win32;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;

namespace AutoCommander.Views.ActionHandlers
{
    /// <summary>
    /// HandHeldMeterExporterLayer.xaml 的交互逻辑
    /// </summary>
    public partial class HandHeldMeterExporterLayer : Window
    {
        private readonly List<HandHeldMeterData> _records;
        private readonly Channel<string> _textChannel;
        private readonly CancellationTokenSource _cancel;

        public string Command { get; set; }

        public HandHeldMeterExporterLayer()
        {
            InitializeComponent();

            _records = new List<HandHeldMeterData>();
            _textChannel = Channel.CreateBounded<string>(64);
            _cancel = new CancellationTokenSource();

            StartChannelReaderTask();
        }

        public async void Receive(string text)
        {
            await _textChannel.Writer.WaitToWriteAsync();
            await _textChannel.Writer.WriteAsync(text);
        }

        private void StartChannelReaderTask()
        {
            Task.Run(async () =>
            {
                string tail = string.Empty;
                while (!_cancel.IsCancellationRequested && await _textChannel.Reader.WaitToReadAsync())
                {
                    tail += await _textChannel.Reader.ReadAsync();
                    var lines = tail.Split('\n');
                    if (lines.Length == 1)
                    {
                        tail = lines[0];
                    }
                    else if (lines.Length > 1)
                    {
                        for (int index = 0; index < lines.Length - 1; ++index)
                        {
                            if (lines[index].StartsWith("Export complete"))
                            {
                                Dispatcher.Invoke(() => ShowOperatePanel());
                                break;
                            }

                            var record = HandHeldMeterData.Parse(lines[index]);
                            if (record != null)
                            {
                                _records.Add(record);
                            }
                        }
                        tail = lines[lines.Length - 1];
                    }
                }
            }, _cancel.Token);
        }

        public void ShowOperatePanel()
        {
            FromTimePicker.Value = DateTime.Now.AddDays(-1);
            ToTimePicker.Value = DateTime.Now;
            TipsTextBlock.Visibility = Visibility.Collapsed;
            OperateStackPanel.Visibility = Visibility.Visible;
        }

        private void ExportButtonClicked(object sender, RoutedEventArgs e)
        {
            _cancel.Cancel();

            Expression<Func<HandHeldMeterData, bool>> exp = r => true;
            if (FromTimePicker.Value.HasValue)
            {
                var fromTime = FromTimePicker.Value.Value;
                Expression<Func<HandHeldMeterData, bool>> xp = r => r.RecordTime >= fromTime;
                exp = exp.And(xp);
            }
            
            if (ToTimePicker.Value.HasValue)
            {
                var toTime = ToTimePicker.Value.Value;
                Expression<Func<HandHeldMeterData, bool>> xp = r => r.RecordTime <= toTime;
                exp = exp.And(xp);
            }

            var predicate = exp.Compile();
            var records = _records.Where(predicate).OrderBy(r => r.RecordTime);

            var dialog = new SaveFileDialog
            {
                Filter = "Excel文件|*.xlsx"
            };
            dialog.ShowDialog();
            if (!string.IsNullOrWhiteSpace(dialog.FileName))
            {
                HandHeldMeterData.Export(dialog.FileName, records);
                Process.Start("Explorer.exe", $@"/select,{dialog.FileName}");
                this.Close();
            }
        }

        class HandHeldMeterData
        {
            public int Floor { get; set; }
            public string Department { get; set; }
            public int Room { get; set; }
            public int BedNumber { get; set; }
            public float Flow { get; set; }
            public float Pressure { get; set; }
            public float Temperature { get; set; }
            public float Humidity { get; set; }
            public DateTime RecordTime { get; set; }

            public static HandHeldMeterData Parse(string text)
            {
                var parts = text.Split(';');
                if (!text.StartsWith("A") || parts.Length != 9) return null;
                var data = new HandHeldMeterData
                {
                    Floor = int.Parse(parts[0].Substring(1)),
                    Department = parts[1].Substring(1),
                    Room = int.Parse(parts[2].Substring(1)),
                    BedNumber = int.Parse(parts[3].Substring(1)),
                    Flow = float.Parse(parts[4].Substring(1)),
                    Pressure = float.Parse(parts[5].Substring(1)),
                    Temperature = float.Parse(parts[6].Substring(1)),
                    Humidity = float.Parse(parts[7].Substring(1))
                };
                var datetime = parts[8].Split(',');
                var date = DateTime.Parse(datetime[0]);
                var time = DateTime.Parse(datetime[1]);
                data.RecordTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
                return data;
            }

            public static void Export(string fileName, IEnumerable<HandHeldMeterData> records)
            {
                using var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("手持仪表数据导出");

                var headerStyle = workbook.HeaderStyle();
                var cellStyle = workbook.BasicStyle();
                var dateStyle = workbook.FormattedStyle("yyyy/MM/DD HH:mm:ss");

                sheet.SetCellValue(0, 0, "楼层", headerStyle);
                sheet.SetCellValue(0, 1, "科室", headerStyle);
                sheet.SetCellValue(0, 2, "病房", headerStyle);
                sheet.SetCellValue(0, 3, "床位", headerStyle);
                sheet.SetCellValue(0, 4, "流量", headerStyle);
                sheet.SetCellValue(0, 5, "压力", headerStyle);
                sheet.SetCellValue(0, 6, "温度", headerStyle);
                sheet.SetCellValue(0, 7, "湿度", headerStyle);
                sheet.SetCellValue(0, 8, "记录时间", headerStyle);

                int row = 1;
                foreach (var record in records)
                {
                    sheet.SetCellValue(row, 0, record.Floor, cellStyle);
                    sheet.SetCellValue(row, 1, record.Department, cellStyle);
                    sheet.SetCellValue(row, 2, record.Room, cellStyle);
                    sheet.SetCellValue(row, 3, record.BedNumber, cellStyle);
                    sheet.SetCellValue(row, 4, record.Flow, cellStyle);
                    sheet.SetCellValue(row, 5, record.Pressure, cellStyle);
                    sheet.SetCellValue(row, 6, record.Temperature, cellStyle);
                    sheet.SetCellValue(row, 7, record.Humidity, cellStyle);
                    sheet.SetCellValue(row, 8, record.RecordTime, dateStyle);
                    row++;
                }

                sheet.AutoSizeColumns(0, 7);
                workbook.Write(stream);
                workbook.Close();
            }
        }

        private void ClosedButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _cancel.Cancel();
        }
    }
}
