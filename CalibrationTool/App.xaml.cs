using System;
using System.Windows;

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
                MessageBox.Show("程序异常：" + e1.Exception.Message + Environment.NewLine + e1.Exception.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            base.OnStartup(e);
        }
    }
}
