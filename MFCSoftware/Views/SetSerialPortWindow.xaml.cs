using System.Windows;

namespace MFCSoftware.Views
{
    /// <summary>
    /// SetSerialPortWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetSerialPortWindow : Window
    {
        public SetSerialPortWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
