using MFCSoftware.ViewModels;
using System.Windows;

namespace MFCSoftware
{
    /// <summary>
    /// AddChannelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddChannelWindow : Window
    {
        private readonly AddChannelWindowViewModel vm;
        public AddChannelWindow()
        {
            InitializeComponent();
            vm = new AddChannelWindowViewModel();
            this.DataContext = vm;
            addressTextBox.Focus();
        }

        public bool IsAddressSetted { get; private set; }
        public int Address { get => vm.Address; }
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsAddressSetted = true;
            this.Close();
        }
    }
}
