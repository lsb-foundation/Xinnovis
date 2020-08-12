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
            main = ViewModelBase.GetViewModelInstance<MainViewModel>() as MainViewModel;
            DataContext = main;
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

        private void AboutMenu_Clicked(object sender, RoutedEventArgs e)
        {
            AboutWindow window = new AboutWindow();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
