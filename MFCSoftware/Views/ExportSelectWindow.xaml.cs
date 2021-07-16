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
        private readonly ExportSelectWindowViewModel _vm;
        public ExportSelectWindow()
        {
            InitializeComponent();
            _vm = this.DataContext as ExportSelectWindowViewModel;
        }

        public bool IsReady { get; private set; }
        public ExportType ExportType { get; set; }
        public DateTime FromTime => _vm.FromDate.AddHours(_vm.FromHour);
        public DateTime ToTime => _vm.ToDate.AddHours(_vm.ToHour);

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)byTimeRadio.IsChecked)
                ExportType = ExportType.ByTime;
            else if ((bool)allRadio.IsChecked)
                ExportType = ExportType.All;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsReady = _vm.FromHour >= 0 && _vm.ToHour >= 0
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
