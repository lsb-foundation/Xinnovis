using MultipleDevicesMonitor.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MultipleDevicesMonitor.Views
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            serialSettingsTab.DataContext = ViewModelBase.GetViewModelInstance<SerialViewModel>();
            softwareSettingsTab.DataContext = ViewModelBase.GetViewModelInstance<SettingsViewModel>();
        }
    }
}
