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

        public static object GetValue(this ICell cell)
        {
            switch (cell.CellType)
            {
                case CellType.Unknown:
                case CellType.Blank:
                    return string.Empty;
                case CellType.Boolean:
                    return cell.BooleanCellValue;
                case CellType.Numeric:
                    return cell.NumericCellValue;
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Formula:
                    return cell.CellFormula;
                case CellType.Error:
                    return cell.ErrorCellValue;
                default:
                    return null;
            }
        }

        public static void SetCellValue(this ISheet sheet, int row, int column, object value, ICellStyle style = null)
        {
            sheet.GetCell(row, column).SetCellValueEx(value, style);
        }

        public static void SetCellValueEx(this ICell cell, object value, ICellStyle style = null)
        {
            if (value == null) return;
            switch (value.GetType().FullName)
            {
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Single":
                case "System.Double":
                case "System.Decimal":
                    cell.SetCellValue(Convert.ToDouble(value));
                    break;
                case "System.DateTime":
                    cell.SetCellValue((DateTime)value);
                    break;
                case "System.Boolean":
                    cell.SetCellValue((bool)value);
                    break;
                case "System.String":
                default:
                    cell.SetCellValue(value as string);
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
    }
}
