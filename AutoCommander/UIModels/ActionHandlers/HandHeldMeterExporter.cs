using AutoCommander.Views.ActionHandlers;
using System;
using System.Windows;

namespace AutoCommander.UIModels.ActionHandlers
{
    public class HandHeldMeterExporter : IActionHandler
    {
        private HandHeldMeterExporterLayer _layer;

        public string Command { get; set; }

        public event Action Completed;

        public void Execute()
        {
            _layer.ShowDialog();    //此处程序将阻塞
            Completed?.Invoke();
        }

        public void Initialize()
        {
            _layer = new HandHeldMeterExporterLayer
            {
                Command = this.Command,
                Owner = Application.Current.MainWindow
            };
        }

        public void Receive(string text)
        {
            _layer.Receive(text);
        }
    }
}
