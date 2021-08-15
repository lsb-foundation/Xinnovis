using CommonLib.Utils;
using System.Windows;

namespace AwesomeCommand
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (s, ex) =>
            {
                LoggerHelper.WriteLog(ex.Exception.Message, ex.Exception);
                _ = MessageBox.Show(ex.Exception.Message);
                ex.Handled = true;
            };
        }
    }
}
