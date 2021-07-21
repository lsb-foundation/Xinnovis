using CommonLib.Utils;
using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

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
                Crashes.TrackError(ex.Exception);
                MessageBox.Show(ex.Exception.Message + "\n" + ex.Exception.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoggerHelper.WriteLog(ex.Exception.Message, ex.Exception);
                ex.Handled = true;
            };
            AppCenter.Start("a09e6616-73c1-4cfe-8ea5-003166764a00", typeof(Analytics), typeof(Crashes));
            base.OnStartup(e);
        }
    }
}
