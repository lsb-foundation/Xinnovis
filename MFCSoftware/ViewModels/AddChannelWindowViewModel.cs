using CommonLib.Mvvm;

namespace MFCSoftware.ViewModels
{
    public class AddChannelWindowViewModel : BindableBase
    {
        private int _address;
        public int Address 
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }
    }
}
