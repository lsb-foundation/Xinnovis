using CommonLib.Extensions;
using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoCommander.UIModels
{
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
            Button button = new Button { Content = Description };
            button.Click += Button_Click;
            return button;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Dictionary<string, string> filtersMap = new Dictionary<string, string>
            {
                ["excel"] = "Excel|*.xlsx;*.xls",
                ["text"] = "文本|*.txt",
                ["json"] = "Json|*.json",
                ["xml"] = "Xml|*.xml"
            };
            OpenFileDialog ofd = new OpenFileDialog { Multiselect = false };
            if (filtersMap.ContainsKey(Type?.ToLower()))
            {
                ofd.Filter = filtersMap[Type.ToLower()];
            }
            bool? result = ofd.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                return;
            }

            foreach (Group group in Parent.Groups)
            {
                foreach (Command command in group.Commands)
                {
                    foreach (Parameter parameter in command.Parameters)
                    {
                        if (parameter.Type.ToLower() == "excel")
                        {
                            using FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                            IWorkbook workbook = WorkbookFactory.Create(fs);
                            AreaReference area = new AreaReference(parameter.DataRange, NPOI.SS.SpreadsheetVersion.EXCEL2007);
                            ISheet sheet = workbook.GetSheet(area.FirstCell.SheetName);
                            List<string> values = new List<string>();
                            foreach (CellReference cref in area.GetAllReferencedCells().OrderBy(c => c.Row).ThenBy(c => c.Col))
                            {
                                string value = sheet?.GetCell(cref.Row, cref.Col).GetValue()?.ToString();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    values.Add(value);
                                }
                            }
                            parameter.Value = string.Join(parameter.Seperator, values);
                            workbook.Close();
                        }
                    }
                }
            }
        }
    }
}
