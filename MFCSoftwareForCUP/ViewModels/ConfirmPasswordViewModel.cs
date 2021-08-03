using CommonLib.MfcUtils;
using GalaSoft.MvvmLight;
using System.Threading.Tasks;

namespace MFCSoftwareForCUP.ViewModels
{
    public class ConfirmPasswordViewModel : ViewModelBase
    {
        #region Fields
        private string password;
        private string labelMessage;
        #endregion

        #region Properties
        public string Password
        {
            get => password;
            set => Set(ref password, value);
        }

        public string LabelMessage
        {
            get => labelMessage;
            set => Set(ref labelMessage, value);
        }

        public bool PasswordConfirmed { get; private set; }
        #endregion

        public async void ConfirmPassword()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                return;
            }

            PasswordConfirmed = await DbStorage.CheckPasswordAsync(Password);
            if (!PasswordConfirmed)
            {
                LabelMessage = "密码错误！";
                await Task.Delay(1000);
                LabelMessage = null;
            }
        }
    }
}
