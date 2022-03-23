using MFCSoftware.Utils;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace MFCSoftwareForCUP.ViewModels
{
    public class ResetPasswordViewModel : ObservableObject
    {
        public ResetPasswordViewModel()
        {
            UpdatePasswordCommand = new RelayCommand(UpdatePassword);
        }

        #region Fields
        private string oldPassword;
        private string newPassword;
        private string labelMessage;
        #endregion

        #region Properties
        public string OldPassword
        {
            get => oldPassword;
            set => SetProperty(ref oldPassword, value);
        }

        public string NewPassword
        {
            get => newPassword;
            set => SetProperty(ref newPassword, value);
        }

        public string LabelMessage
        {
            get => labelMessage;
            set => SetProperty(ref labelMessage, value);
        }

        public ICommand UpdatePasswordCommand { get; }

        public SolidColorBrush LabelColor { get; } = new SolidColorBrush(Colors.Green);
        #endregion

        #region Methods
        private async void UpdatePassword()
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                return;
            }
            if (await SqliteHelper.CheckPasswordAsync(oldPassword))
            {
                await SqliteHelper.UpdatePasswordAsync(newPassword);
                SetLabelMessage("密码修改成功。", true);
            }
            else
            {
                SetLabelMessage("原密码错误，请重试！", false);
            }
        }

        private async void SetLabelMessage(string message, bool success)
        {
            LabelColor.Color = success ? Colors.Green : Colors.Red;
            LabelMessage = message;
            await Task.Delay(1000);
            LabelMessage = null;
        }
        #endregion
    }
}
