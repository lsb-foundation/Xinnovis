using NPOI.SS.UserModel;

namespace MFCSoftware.Utils
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

        public static ICellStyle FormattedStyle(this IWorkbook workbook, string formatter = "0.000", bool isBold = false, double fontSize = 11.0)
        {
            ICellStyle style = workbook.BasicStyle();
            IDataFormat format = workbook.CreateDataFormat();
            style.DataFormat = format.GetFormat(formatter);
            IFont font = style.GetFont(workbook);
            font.IsBold = isBold;
            font.FontHeightInPoints = fontSize;
            return style;
        }
    }
}
