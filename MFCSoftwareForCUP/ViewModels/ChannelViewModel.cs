using MFCSoftwareForCUP.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Configuration;
using System.Windows.Media;

namespace MFCSoftwareForCUP.ViewModels
{
    public class ChannelViewModel : ObservableObject
    {
        #region Fields
        private float accuFlow;
        private float currFlow;
        private string accuFlowUnit;
        private DeviceExtras _deviceExtras = new DeviceExtras();
        #endregion

        #region Properties
        public int Address
        {
            get => _deviceExtras.Address;
            set
            {
                _deviceExtras.Address = value;
                OnPropertyChanged();
            }
        }

        public string Floor
        {
            get => _deviceExtras.Floor;
            set
            {
                _deviceExtras.Floor = value;
                OnPropertyChanged();
            }
        }

        public string Room
        {
            get => _deviceExtras.Room;
            set
            {
                _deviceExtras.Room = value;
                OnPropertyChanged();
            }
        }

        public string GasType
        {
            get => _deviceExtras.GasType;
            set
            {
                _deviceExtras.GasType = value;
                OnPropertyChanged();
            }
        }

        public float AccumulateFlow
        {
            get => accuFlow;
            set => SetProperty(ref accuFlow, value);
        }

        public string AccumulateFlowUnit
        {
            get => accuFlowUnit;
            set => SetProperty(ref accuFlowUnit, value);
        }

        public float CurrentFlow
        {
            get => currFlow;
            set => SetProperty(ref currFlow, value);
        }

        public string CurrentFlowUnit { get; } = ConfigurationManager.AppSettings["瞬时流量单位"];

        public SolidColorBrush StatusColor { get; } = new SolidColorBrush(Colors.Green);

        public DeviceExtras DeviceExtras => _deviceExtras;
        #endregion

        public void SetExtras(DeviceExtras extras)
        {
            _deviceExtras = extras;
            OnPropertyChanged(nameof(Floor));
            OnPropertyChanged(nameof(Room));
            OnPropertyChanged(nameof(GasType));
        }
    }
}
