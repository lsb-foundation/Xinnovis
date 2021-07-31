﻿using GalaSoft.MvvmLight;
using MFCSoftwareForCUP.Models;
using System.Windows.Media;

namespace MFCSoftwareForCUP.ViewModels
{
    public class ChannelViewModel : ViewModelBase
    {
        #region Fields
        private DeviceExtras _deviceExtras = new DeviceExtras();
        private float accuFlow;
        private float currFlow;
        private string accuFlowUnit;
        private string currFlowUnit;
        private SolidColorBrush statusColor = new SolidColorBrush(Colors.Green);
        #endregion

        #region Properties
        public int Address
        {
            get => _deviceExtras.Address;
            set
            {
                _deviceExtras.Address = value;
                RaisePropertyChanged();
            }
        }

        public string Floor
        {
            get => _deviceExtras.Floor;
            set
            {
                _deviceExtras.Floor = value;
                RaisePropertyChanged();
            }
        }

        public string Room
        {
            get => _deviceExtras.Room;
            set
            {
                _deviceExtras.Room = value;
                RaisePropertyChanged();
            }
        }

        public string GasType
        {
            get => _deviceExtras.GasType;
            set
            {
                _deviceExtras.GasType = value;
                RaisePropertyChanged();
            }
        }

        public float AccumulateFlow
        {
            get => accuFlow;
            set => Set(ref accuFlow, value);
        }

        public string AccumulateFlowUnit
        {
            get => accuFlowUnit;
            set => Set(ref accuFlowUnit, value);
        }

        public float CurrentFlow
        {
            get => currFlow;
            set => Set(ref currFlow, value);
        }

        public string CurrentFlowUnit
        {
            get => currFlowUnit;
            set => Set(ref currFlowUnit, value);
        }

        public SolidColorBrush StatusColor
        {
            get => statusColor;
            set => Set(ref statusColor, value);
        }

        public DeviceExtras DeviceExtras => _deviceExtras;
        #endregion

        public void SetExtras(DeviceExtras extras)
        {
            _deviceExtras = extras;
            RaisePropertyChanged(nameof(Floor));
            RaisePropertyChanged(nameof(Room));
            RaisePropertyChanged(nameof(GasType));
        }
    }
}
