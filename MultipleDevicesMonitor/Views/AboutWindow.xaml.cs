using MultipleDevicesMonitor.ViewModels;
using System.Windows;

namespace MultipleDevicesMonitor.Views
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            DataContext = ViewModelBase.GetViewModelInstance<AboutViewModel>();
        }
    }
}
