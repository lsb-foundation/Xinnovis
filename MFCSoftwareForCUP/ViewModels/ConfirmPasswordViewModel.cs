using MFCSoftware.Utils;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MFCSoftwareForCUP.ViewModels
{
    public class ConfirmPasswordViewModel : ObservableObject
    {
        #region Fields
        private string password;
        private string labelMessage;
        #endregion

        #region Properties
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        public string LabelMessage
        {
            get => labelMessage;
            set => SetProperty(ref labelMessage, value);
        }

        public bool PasswordConfirmed { get; private set; }
        #endregion

        public async void ConfirmPassword()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                return;
            }

            PasswordConfirmed = await SqliteHelper.CheckPasswordAsync(Password);
            if (!PasswordConfirmed)
            {
                LabelMessage = "密码错误！";
                await Task.Delay(1000);
                LabelMessage = null;
            }
        }
    }
}
