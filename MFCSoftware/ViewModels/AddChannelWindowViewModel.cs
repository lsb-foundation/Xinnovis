using GalaSoft.MvvmLight;

namespace MFCSoftware.ViewModels
{
    public class AddChannelWindowViewModel : ViewModelBase
    {
        private int _address;
        public int Address 
        {
            get => _address;
            set => Set(ref _address, value);
        }
    }
}
