using AutoCommander.Properties;
using CommonLib.Extensions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AutoCommander.Views
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
        }

        private void UpdatePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string originPassword = OriginPassword.Password;
            string newPassword = NewPassword.Password;
            if (!string.IsNullOrEmpty(originPassword) && !string.IsNullOrEmpty(newPassword))
            {
                if (Settings.Default.Password == originPassword.MD5HashString())
                {
                    Settings.Default.Password = newPassword.MD5HashString();
                    Settings.Default.Save();
                    Settings.Default.Reload();
                    SetTips("密码更新成功", Colors.Green);
                }
                else
                {
                    SetTips("原密码错误！", Colors.Red);
                }
            }
        }

        private async void SetTips(string tips, Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            TipsLabel.Foreground = brush;
            TipsLabel.Content = tips;
            await Task.Delay(1000);
            TipsLabel.Content = null;
        }
    }
}
