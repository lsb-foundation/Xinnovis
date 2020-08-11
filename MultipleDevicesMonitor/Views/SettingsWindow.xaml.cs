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
        private readonly SettingsViewModel settings;

        public SettingsWindow()
        {
            InitializeComponent();
            settings = ViewModelBase.GetViewModelInstance<SettingsViewModel>() as SettingsViewModel;
            DataContext = settings;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var mainVm = ViewModelBase.GetViewModelInstance<MainViewModel>() as MainViewModel;
            mainVm?.UpdateAxisTitle();
        }
    }
}
