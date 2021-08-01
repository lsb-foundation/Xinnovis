using System.Text;
using System.Windows;
using System.Windows.Threading;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace MFCSoftware
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application 
    {
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Crashes.TrackError(e.Exception);
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("程序发生了尚未捕获到的异常：" + e.Exception.Message)
                .AppendLine(e.Exception.StackTrace);
            MessageBox.Show(messageBuilder.ToString(), "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppCenter.Start("ead83755-b5d7-474e-961f-a5e3aba6fabf", typeof(Analytics), typeof(Crashes));
            base.OnStartup(e);
        }
    }
}
