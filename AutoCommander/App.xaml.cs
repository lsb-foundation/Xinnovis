using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using HandyControl.Controls;
using CommonLib.Utils;

namespace AutoCommander;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppCenter.Start("714179aa-3a75-4b0f-bfb2-0243d9367fdc", typeof(Analytics), typeof(Crashes));
        Current.DispatcherUnhandledException += (s, ex) =>
        {
            Crashes.TrackError(ex.Exception);

            LoggerHelper.WriteLog(ex.Exception.Message, ex.Exception);

            string message = ex.Exception.InnerException != null ?
                ex.Exception.InnerException.Message : ex.Exception.Message;

            Growl.Error(message);
            ex.Handled = true;
        };
        base.OnStartup(e);
    }
}
