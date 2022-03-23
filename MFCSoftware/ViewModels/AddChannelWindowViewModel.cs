using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MFCSoftware.ViewModels
{
    public class AddChannelWindowViewModel : ObservableObject
    {
        private int _address;
        public int Address 
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }
    }
}
