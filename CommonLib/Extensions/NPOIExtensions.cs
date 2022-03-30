using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;

namespace CommonLib.Extensions
{
    public static class NPOIExtensions
    {
        public static IRow GetRowEx(this ISheet sheet, int row)
        {
            return sheet.GetRow(row) ?? sheet.CreateRow(row);
        }

        public static ICell GetCellEx(this IRow row, int column)
        {
            return row.GetCell(column) ?? row.CreateCell(column);
        }

        public static ICell GetCell(this ISheet sheet, int row, int column)
        {
            return sheet.GetRowEx(row).GetCellEx(column);
        }

        public static object GetValue(this ICell cell) =>
            cell.CellType switch
            {
                CellType.Blank => string.Empty,
                CellType.Boolean => cell.BooleanCellValue,
                CellType.Numeric => cell.NumericCellValue,
                CellType.String => cell.StringCellValue,
                CellType.Formula => cell.CellFormula,
                CellType.Error => cell.ErrorCellValue,
                _ => null
            };

        public static void SetCellValue(this ISheet sheet, int row, int column, object value, ICellStyle style = null)
        {
            sheet.GetCell(row, column).SetCellValueEx(value, style);
        }

        public static void SetCellValueEx(this ICell cell, object value, ICellStyle style = null)
        {
            if (value == null) return;
            switch (value)
            {
                case short:
                case int:
                case long:
                case float:
                case double:
                case decimal:
                    cell.SetCellValue(Convert.ToDouble(value));
                    break;
                case DateTime dtVal:
                    cell.SetCellValue(dtVal);
                    break;
                case bool bval:
                    cell.SetCellValue(bval);
                    break;
                case string sval:
                    cell.SetCellValue(sval);
                    break;
                default:
                    break;
            }
            
            if (style != null)
            {
                cell.CellStyle = style;
            }
        }

        public static void AutoSizeColumns(this ISheet sheet, int startColumnIndex, int lastColumnIndex)
        {
            for (int idx = startColumnIndex; idx <= lastColumnIndex; ++idx)
            {
                sheet.AutoSizeColumn(idx);
            }
        }

        public static void MergeRegion(this ISheet sheet, int firstRow, int lastRow, int firstColumn, int lastColumn, object value, ICellStyle style = null)
        {
            if (firstRow == lastRow && firstColumn == lastColumn)
            {
                sheet.GetCell(firstRow, firstColumn).SetCellValueEx(value, style);
                return;
            }
            sheet.AddMergedRegion(new CellRangeAddress(firstRow, lastRow, firstColumn, lastColumn));
            sheet.GetCell(firstRow, firstColumn).SetCellValueEx(value, style);
            if (style == null)
            {
                return;
            }

            for (int row = firstRow; row < lastRow; row++)
            {
                for (int col = firstColumn; col < lastColumn; col++)
                {
                    sheet.GetCell(row, col).CellStyle = style;
                }
            }
        }

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

        public static double EvaluateFormula(this ICell cell)
        {
            if (cell.CellType != CellType.Formula) return 0;
            var evaluator = cell.Sheet.Workbook.GetCreationHelper().CreateFormulaEvaluator();
            CellType type = evaluator.EvaluateFormulaCell(cell);
            return type == CellType.Numeric ? cell.NumericCellValue : 0;
        }
    }
}
