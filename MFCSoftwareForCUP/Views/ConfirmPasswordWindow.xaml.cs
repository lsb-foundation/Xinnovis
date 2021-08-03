using MFCSoftwareForCUP.ViewModels;
using System.Windows;

namespace MFCSoftwareForCUP.Views
{
    /// <summary>
    /// ConfirmPasswordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfirmPasswordWindow : Window
    {
        private readonly ConfirmPasswordViewModel _confirm;

        public ConfirmPasswordWindow()
        {
            InitializeComponent();
            _confirm = DataContext as ConfirmPasswordViewModel;
        }

        #region Properties
        public bool PasswordConfirmed => _confirm.PasswordConfirmed;
        #endregion

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            _confirm.ConfirmPassword();
            if (_confirm.PasswordConfirmed)
            {
                Close();
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e) => Close();
    }
}
