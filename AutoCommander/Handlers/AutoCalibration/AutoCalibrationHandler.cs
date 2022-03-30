using System;
using AutoCommander.AutoUI.Linkers;
using AutoCommander.Handlers.AutoCalibration.Views;
using CommonLib.Utils;

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
        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            AutoCalibrationExporter exporter = new();
            exporter.SetContainer(_collector.Container);
            HandyControl.Controls.Dialog.Show(exporter, "BEB7FC");
            Completed?.Invoke();
        });
    }

    public void Receive(string text)
    {
        _collector.Insert(text);
    }
}
