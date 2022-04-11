using CommonLib.Extensions;
using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoCommander.AutoUI.Models;

public class FileReader : IAutoBuild<Button>
{
    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public string Description { get; set; }

    [XmlAttribute]
    public string Type { get; set; }

    [XmlIgnore]
    public Tab Parent { get; set; }

    public Button Build()
    {
        Button button = new() { Content = Description };
        button.Click += Button_Click;
        return button;
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Dictionary<string, string> filtersMap = new()
        {
            ["excel"] = "Excel|*.xlsx;*.xls",
            ["text"] = "文本|*.txt",
            ["json"] = "Json|*.json",
            ["xml"] = "Xml|*.xml"
        };
        OpenFileDialog ofd = new() { Multiselect = false };
        if (filtersMap.ContainsKey(Type?.ToLower()))
        {
            ofd.Filter = filtersMap[Type.ToLower()];
        }
        bool? result = ofd.ShowDialog();
        if (!result.HasValue || !result.Value)
        {
            return;
        }

        var parameters = Parent.Groups.SelectMany(g => g.Commands.SelectMany(c => c.Parameters));
        foreach (Parameter parameter in parameters)
        {
            if (parameter.Type.ToLower() != "excel") continue;

            using FileStream stream = new(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var workbook = WorkbookFactory.Create(stream);
            var area = new AreaReference(parameter.DataRange, NPOI.SS.SpreadsheetVersion.EXCEL2007);
            var sheet = workbook.GetSheet(area.FirstCell.SheetName);
            var values = new List<string>();
            foreach (var cref in area.GetAllReferencedCells().OrderBy(c => c.Row).ThenBy(c => c.Col))
            {
                var cell = sheet?.GetCell(cref.Row, cref.Col);
                if (cell is null) continue;
                var value = cell.CellType switch
                {
                    CellType.Numeric => cell.NumericCellValue.ToString("f5"),
                    CellType.Formula => cell.EvaluateFormula().ToString("f5"),
                    _ => cell.GetValue().ToString()
                };
                if (!string.IsNullOrEmpty(value))
                {
                    values.Add(value);
                }
            }
            string text = string.Join(parameter.Seperator, values);
            parameter.SetTextBox(text);
            workbook.Close();
        }
    }
}
