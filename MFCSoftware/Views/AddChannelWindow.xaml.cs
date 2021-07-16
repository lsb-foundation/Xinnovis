using MFCSoftware.ViewModels;
using System.Windows;

namespace MFCSoftware.Views
{
    /// <summary>
    /// AddChannelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddChannelWindow : Window
    {
        private readonly AddChannelWindowViewModel _vm;
        public AddChannelWindow()
        {
            InitializeComponent();
            addressTextBox.Focus();
            _vm = this.DataContext as AddChannelWindowViewModel;
        }

        public bool IsAddressSetted { get; private set; }
        public int Address { get => _vm.Address; }
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsAddressSetted = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            addressTextBox.SelectAll();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                Button_Click(null, null);
            }
        }
    }
}
