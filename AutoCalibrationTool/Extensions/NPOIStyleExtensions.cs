using NPOI.SS.UserModel;

namespace AutoCalibrationTool.Extensions
{
    public static class NPOIStyleExtensions
    {
        public static ICellStyle BasicStyle(this IWorkbook workbook)
        {
            ICellStyle style = workbook.CreateCellStyle();
            IFont font = workbook.CreateFont();
            font.FontHeightInPoints = 11;
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            style.SetFont(font);
            return style;
        }

        public static ICellStyle HeaderStyle(this IWorkbook workbook)
        {
            ICellStyle style = workbook.BasicStyle();
            IFont font = style.GetFont(workbook);
            font.IsBold = true;
            font.FontHeightInPoints = 11.0;
            return style;
        }

        public static ICellStyle BasicNumericStyle(this IWorkbook workbook, string formatter = "0.000")
        {
            ICellStyle style = workbook.BasicStyle();
            IDataFormat format = workbook.CreateDataFormat();
            style.DataFormat = format.GetFormat(formatter);
            return style;
        }

        public static ICellStyle TemperatureCellStyle(this IWorkbook workbook)
        {
            ICellStyle style = workbook.BasicNumericStyle("0.0");
            IFont font = style.GetFont(workbook);
            font.IsBold = true;
            font.FontHeightInPoints = 14.0;
            return style;
        }

        public static ICellStyle AverageCellStyle(this IWorkbook workbook)
        {
            ICellStyle style = workbook.BasicNumericStyle();
            IFont font = style.GetFont(workbook);
            font.IsBold = true;
            font.FontHeightInPoints = 11.0;
            return style;
        }
    }
}
