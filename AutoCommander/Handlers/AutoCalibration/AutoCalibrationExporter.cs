using AutoCommander.Handlers.AutoCalibration.Models;
using CommonLib.Extensions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoCommander.Handlers.AutoCalibration;

public class AutoCalibrationExporter
{
    public void Export(CalibrationDataCollection list, string file)
    {
        using var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        IWorkbook workbook = new XSSFWorkbook();

        BuildSheet(list, workbook);

        workbook.Write(fs);
        workbook.Close();
    }

    private void BuildSheet(CalibrationDataCollection list, IWorkbook workbook)
    {
        if (list.Count == 0) return;

        var orderedDeviceDatas = list.OrderBy(dd => dd.SerialNumber)
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
