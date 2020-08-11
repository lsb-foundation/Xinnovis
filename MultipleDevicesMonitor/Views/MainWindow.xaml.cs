using MultipleDevicesMonitor.ViewModels;
using System.Windows;
using MultipleDevicesMonitor.Properties;

namespace MultipleDevicesMonitor.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel main;

        public MainWindow()
        {
            InitializeComponent();
            main = ViewModelManager.GetViewModelInstance<MainViewModel>();
            DataContext = main;
        }

        private void AddDeviceButton_Clicked(object sender, RoutedEventArgs e)
        {
            main.AddNewDevice();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
        }

        private void SettingsMenu_Clicked(object sender, RoutedEventArgs e)
        {
            SettingsWindow win = new SettingsWindow();
            win.Owner = this;
            win.ShowDialog();
        }
    }
}
