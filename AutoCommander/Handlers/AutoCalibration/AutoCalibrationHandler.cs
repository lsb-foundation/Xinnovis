using System;
using AutoCommander.AutoUI.Linkers;
using AutoCommander.Handlers.AutoCalibration.Views;

namespace AutoCommander.Handlers.AutoCalibration;

public class AutoCalibrationHandler : IActionHandler
{
    private readonly AutoCalibrationCollector _collector = new();

    public event Action Completed;

    public void Initialize()
    {
        _collector.Completed += Collector_Completed;    //处理完成，自动导出
    }

    private void Collector_Completed()
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            AutoCalibrationExporter exporter = new();
            exporter.SetCollection(_collector.Collection);
            HandyControl.Controls.Dialog.Show(exporter);
            Completed?.Invoke();
        }));
    }

    public void Receive(string text)
    {
        _collector.Insert(text);
    }
}
