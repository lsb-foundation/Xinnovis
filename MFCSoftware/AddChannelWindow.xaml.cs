using MFCSoftware.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MFCSoftware
{
    /// <summary>
    /// AddChannelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddChannelWindow : Window
    {
        private AddChannelWindowViewModel vm;
        public AddChannelWindow()
        {
            InitializeComponent();
            vm = new AddChannelWindowViewModel();
            this.DataContext = vm;
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
