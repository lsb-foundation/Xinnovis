using CommonLib.Extensions;
using CommonLib.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MFCSoftware.Utils
{
    public class FlowData
    {
        #region Properties
        public int Address { get; set; }
        public float CurrentFlow { get; set; }
        public string Unit { get; set; }
        public float AccuFlow { get; set; }
        public string AccuFlowUnit { get; set; }
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public DateTime CollectTime { get; set; }   //导出数据时用到该属性
        #endregion

        #region Methods
        private static readonly List<UnitCode> _unitCodes = UnitCode.GetUnitCodesFromConfiguration();
        public static FlowData ResolveFromBytes(byte[] data)
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
                Unit = _unitCodes.FirstOrDefault(u => u.Code == unitCode)?.Unit,
                AccuFlow = accuFlow,
                AccuFlowUnit = unit,
                Days = days,
                Hours = hours,
                Minutes = mins,
                Seconds = secs
            };

            return flowData;
        }

        public static void ExportFlowDatas(string fileName, List<FlowData> flowDatas)
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
        #endregion
    }
}
