using AutoCommander.Properties;
using CommonLib.Extensions;
using System.Threading.Tasks;
using System.Windows;

namespace AutoCommander.Views
{
    /// <summary>
    /// ConfirmPasswordDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConfirmPasswordDialog : Window
    {
        public ConfirmPasswordDialog()
        {
            InitializeComponent();
        }

        public bool Confirmed { get; set; }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Password.Password))
            {
                if (Settings.Default.Password == Password.Password.MD5HashString())
                {
                    Confirmed = true;
                    Close();
                }
                else
                {
                    Confirmed = false;
                    TipsLabel.Content = "密码错误！";
                    await Task.Delay(1000);
                    TipsLabel.Content = null;
                }
            }
        }
    }
}
