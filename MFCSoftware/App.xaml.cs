using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MFCSoftware
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application 
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("程序发生了尚未捕获到的异常：" + e.Exception.Message)
                .AppendLine(e.Exception.StackTrace);
            MessageBox.Show(messageBuilder.ToString(), "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            App.Current.Shutdown();
            e.Handled = true;
        }
    }
}
