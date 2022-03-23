using CommonLib.Extensions;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;

namespace AutoCommander.Handlers.HandHeldMeter.Views;

/// <summary>
/// HandHeldMeterExporterLayer.xaml 的交互逻辑
/// </summary>
public partial class HandHeldMeterExporter
{
    private List<HandHeldMeterData> dataRecords;

    public HandHeldMeterExporter()
    {
        InitializeComponent();
        FromTimePicker.DisplayDateTime = DateTime.Now.AddDays(-1);
        FromTimePicker.SelectedDateTime = FromTimePicker.DisplayDateTime;
        ToTimePicker.DisplayDateTime = DateTime.Now;
        ToTimePicker.SelectedDateTime = ToTimePicker.DisplayDateTime;
    }

    public void SetDataRecords(List<HandHeldMeterData> records)
    {
        dataRecords = records;
    }

    private void ExportButtonClicked(object sender, RoutedEventArgs e)
    {
        Expression<Func<HandHeldMeterData, bool>> exp = r => true;
        if (FromTimePicker.SelectedDateTime.HasValue)
        {
            var fromTime = FromTimePicker.SelectedDateTime.Value;
            Expression<Func<HandHeldMeterData, bool>> xp = r => r.RecordTime >= fromTime;
            exp = exp.And(xp);
        }
        
        if (ToTimePicker.SelectedDateTime.HasValue)
        {
            var toTime = ToTimePicker.SelectedDateTime.Value;
            Expression<Func<HandHeldMeterData, bool>> xp = r => r.RecordTime <= toTime;
            exp = exp.And(xp);
        }

        var predicate = exp.Compile();
        var records = dataRecords.Where(predicate).OrderBy(r => r.RecordTime);

        var dialog = new SaveFileDialog
        {
            Filter = "Excel文件|*.xlsx"
        };
        dialog.ShowDialog();
        if (!string.IsNullOrWhiteSpace(dialog.FileName))
        {
            HandHeldMeterDataExporter.Export(dialog.FileName, records);
            Process.Start("Explorer.exe", $@"/select,{dialog.FileName}");
        }
    }
}
