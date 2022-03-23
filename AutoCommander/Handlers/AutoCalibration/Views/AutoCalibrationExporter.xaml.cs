using AutoCommander.Handlers.AutoCalibration.Models;
using CommonLib.Extensions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace AutoCommander.Handlers.AutoCalibration.Views
{
    /// <summary>
    /// AutoCalibrationExporter.xaml 的交互逻辑
    /// </summary>
    public partial class AutoCalibrationExporter
    {
        private CalibrationDataCollection collection;

        public AutoCalibrationExporter()
        {
            InitializeComponent();
        }

        public void SetCollection(CalibrationDataCollection collection)
        {
            this.collection = collection;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            using FolderBrowserDialog dialog = new() { Description = "选择标定文件导出位置" };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            string file = Path.Combine(dialog.SelectedPath, $"自动标定-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            using var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            IWorkbook workbook = new XSSFWorkbook();
            BuildSheet(workbook);
            workbook.Write(fs);
            workbook.Close();

            Process.Start("Explorer.exe", $@"/select,{file}");
        }

        private void BuildSheet(IWorkbook workbook)
        {
            if (collection.Count == 0) return;

            var orderedDeviceDatas = collection.OrderBy(dd => dd.SerialNumber)
                    .Select((v, i) => (Index: i, Data: v))
                    .GroupBy(t => t.Index / 8)
                    .Select(g => g.Select(t => t.Data).ToList());

            ISheet sheet = workbook.CreateSheet();
            ICellStyle headerStyle = workbook.HeaderStyle();
            ICellStyle basicNumericStyle = workbook.FormattedStyle();
            ICellStyle temperatureStyle = workbook.FormattedStyle(formatter: "0.0", isBold: true, fontSize: 14.0);
            ICellStyle averageStyle = workbook.FormattedStyle(isBold: true);
            ICellStyle flowStyle = workbook.FormattedStyle(formatter: "0.0", isBold: true, fontSize: 11.0);

            int row = 0;
            foreach (IEnumerable<CalibrationData> datas in orderedDeviceDatas)
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
                sheet.SetCellValue(row + 4, 0, "时间差", headerStyle);
                sheet.SetCellValue(row + 5, 0, "最大值", headerStyle);
                sheet.SetCellValue(row + 6, 0, "最小值", headerStyle);
                sheet.SetCellValue(row + 7, 0, "差值", headerStyle);
                sheet.SetCellValue(row + 8, 0, "平均值", headerStyle);
                #endregion

                for (int deviceDataIndex = 0; deviceDataIndex < datas.Count(); ++deviceDataIndex)
                {
                    ++column;
                    var deviceData = datas.ElementAt(deviceDataIndex);

                    sheet.MergeRegion(deviceCodeRow, deviceCodeRow, column, column + deviceData.FlowDatas.Count - 1, deviceData.SerialNumber, headerStyle);   //写入设备号

                    foreach (CalibrationVoltData flowData in deviceData.FlowDatas.OrderBy(ftd => ftd.Flow))
                    {
                        row = deviceCodeRow;

                        sheet.SetCellValue(++row, column, flowData.Flow, flowStyle);
                        sheet.SetCellValue(++row, column, flowData.Temperature, temperatureStyle);
                        sheet.SetCellValue(++row, column, flowData.TimespanSeconds);

                        #region 写入公式
                        string range = $"{sheet.GetCell(row + 6, column).Address}:{sheet.GetCell(row + 6 + flowData.Volts.Count - 1, column).Address}";

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
    }
}
