using System;
using System.Windows;
using MFCSoftware.ViewModels;

namespace MFCSoftware.Views
{
    /// <summary>
    /// ExportSelectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExportSelectWindow : Window
    {
        private ExportSelectWindowViewModel viewModel = new ExportSelectWindowViewModel();
        public ExportSelectWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public bool IsReady { get; private set; }
        public ExportType ExportType { get; set; }
        public DateTime FromTime => viewModel.FromDate.AddHours(viewModel.FromHour);
        public DateTime ToTime => viewModel.ToDate.AddHours(viewModel.ToHour);

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)byTimeRadio.IsChecked)
                ExportType = ExportType.ByTime;
            else if ((bool)allRadio.IsChecked)
                ExportType = ExportType.All;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsReady = viewModel.FromHour >= 0 && viewModel.ToHour >= 0
                && FromTime < ToTime;
            if (IsReady) this.Close();
            else MessageBox.Show("请检查日期是否正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public enum ExportType
    {
        All,
        ByTime
    }
}
