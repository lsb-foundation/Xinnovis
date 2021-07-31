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
            AppCenter.Start("b85fe049-95eb-4110-b438-503b8f6c6e5f",
                   typeof(Analytics), typeof(Crashes));
            base.OnStartup(e);
        }
    }
}
