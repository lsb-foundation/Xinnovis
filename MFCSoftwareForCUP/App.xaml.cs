using CommonLib.Utils;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Windows;

namespace MFCSoftwareForCUP
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Dispatcher.UnhandledException += (s, ex) =>
            {
                LoggerHelper.WriteLog(ex.Exception.Message, ex.Exception);
                Crashes.TrackError(ex.Exception);
                _ = MessageBox.Show("程序发生了尚未捕获到的异常：" + ex.Exception.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                Current.Shutdown();
                //throw ex.Exception;
            };
            AppCenter.Start("b85fe049-95eb-4110-b438-503b8f6c6e5f",
                   typeof(Analytics), typeof(Crashes));
            base.OnStartup(e);
        }
    }
}
