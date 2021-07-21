using System;
using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace CalibrationTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (s, e1) =>
            {
                Crashes.TrackError(e1.Exception);
                MessageBox.Show("程序异常：" + e1.Exception.Message + Environment.NewLine + e1.Exception.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            AppCenter.Start("e9a0ff4e-0773-4422-9673-24ce74b8d7e5", typeof(Analytics), typeof(Crashes));
            base.OnStartup(e);
        }
    }
}
