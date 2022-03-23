using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using AutoCommander.AutoUI.Linkers;

namespace AutoCommander.Handlers.AutoCalibration;

public class AutoCalibrationHandler : IActionHandler
{
    private readonly AutoCalibrationCollector _collector = new();

    public string Command { get; set; }

    public event Action Completed;

    public void Execute() { }

    public void Initialize()
    {
        _collector.Completed += Collector_Completed;    //处理完成，自动导出
    }

    private void Collector_Completed()
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            using FolderBrowserDialog dialog = new() { Description = "选择标定文件导出位置" };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            string file = Path.Combine(dialog.SelectedPath, $"自动标定-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            new AutoCalibrationExporter().Export(_collector.Collection, file);

            Process.Start("Explorer.exe", $@"/select,{file}");
            Completed?.Invoke();
        }));
    }

    public void Receive(string text)
    {
        _collector.Insert(text);
    }
}
