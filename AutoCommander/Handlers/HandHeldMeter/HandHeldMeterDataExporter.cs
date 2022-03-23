using CommonLib.Extensions;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;

namespace AutoCommander.Handlers.HandHeldMeter;

public class HandHeldMeterDataExporter
{
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
