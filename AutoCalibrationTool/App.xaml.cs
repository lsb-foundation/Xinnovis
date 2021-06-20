using CommonLib.Utils;
using System.Windows;

namespace AutoCalibrationTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += (s, ex) =>
            {
                MessageBox.Show(ex.Exception.Message + "\n" + ex.Exception.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoggerHelper.WriteLog(ex.Exception.Message, ex.Exception);
                ex.Handled = true;
            };
            base.OnStartup(e);
        }
    }
}
