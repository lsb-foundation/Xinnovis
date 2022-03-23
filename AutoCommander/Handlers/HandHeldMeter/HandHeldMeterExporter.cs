using AutoCommander.AutoUI.Linkers;
using System;
using System.Windows;

namespace AutoCommander.Handlers.HandHeldMeter;

public class HandHeldMeterExporter : IActionHandler
{
    private HandHeldMeterDataCollector collector;

    public string Command { get; set; }

    public event Action Completed;

    public void Execute() { }

    public void Initialize()
    {
        collector = new();
        collector.Completed += () =>
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Views.HandHeldMeterExporter exporter = new();
                exporter.SetDataRecords(collector.Records);
                HandyControl.Controls.Dialog.Show(exporter);
            }));
            Completed?.Invoke();
        };
    }

    public void Receive(string text)
    {
        collector.Insert(text);
    }
}
