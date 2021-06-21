using AutoCalibrationTool.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace AutoCalibrationTool.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            IncubeStartCommand = new RelayCommand(() => Send("INCUBE_START!", CommandType.IncubeStart));
            IncubeStopCommand = new RelayCommand(() => Send("INCUBE_STOP!", CommandType.IncubeStop));
            RoomStartCommand = new RelayCommand(() => Send("ROOM_START!", CommandType.RoomStart));
            RoomStopCommand = new RelayCommand(() => Send("ROOM_STOP!", CommandType.RoomStop));
        }

        #region Properties
        public bool IncubeStartButtonEnabled => ViewModelLocator.Port.IsOpen && Mode == CalibrationMode.Stop;
        public bool IncubeStopButtonEnabled => ViewModelLocator.Port.IsOpen && Mode == CalibrationMode.Incube;
        public bool RoomStartButtonEnabled => ViewModelLocator.Port.IsOpen && Mode == CalibrationMode.Stop;
        public bool RoomStopButtonEnabled => ViewModelLocator.Port.IsOpen && Mode == CalibrationMode.Room;
        public CalibrationMode Mode { get; private set; } = CalibrationMode.Stop;
        public int DeviceDataCount { get; private set; }
        public int FlowDataCount { get; private set; }
        #endregion

        #region Commands
        public RelayCommand IncubeStartCommand { get; }
        public RelayCommand IncubeStopCommand { get; }
        public RelayCommand RoomStartCommand { get; }
        public RelayCommand RoomStopCommand { get; }
        #endregion

        #region Methods
        public void UpdateButtonEnableStatus()
        {
            RaisePropertyChanged(nameof(IncubeStartButtonEnabled));
            RaisePropertyChanged(nameof(IncubeStopButtonEnabled));
            RaisePropertyChanged(nameof(RoomStartButtonEnabled));
            RaisePropertyChanged(nameof(RoomStopButtonEnabled));
        }

        public void SetDataCount(int deviceDataCount, int flowDataCount)
        {
            DeviceDataCount = deviceDataCount;
            FlowDataCount = flowDataCount;
            RaisePropertyChanged(nameof(DeviceDataCount));
            RaisePropertyChanged(nameof(FlowDataCount));
        }

        private void Send(string content, CommandType type)
        {
            ViewModelLocator.Port.Send(content);
            switch (type)
            {
                case CommandType.IncubeStart:
                    Mode = CalibrationMode.Incube;
                    break;
                case CommandType.IncubeStop:
                    Mode = CalibrationMode.Stop;
                    break;
                case CommandType.RoomStart:
                    Mode = CalibrationMode.Room;
                    break;
                case CommandType.RoomStop:
                    Mode = CalibrationMode.Stop;
                    break;
            }
            UpdateButtonEnableStatus();
            ViewModelLocator.Port.UpdatePortButtonStatus();
            ViewModelLocator.Storage.UpdateButtonStatus();
        }
        #endregion

        enum CommandType
        {
            IncubeStart,
            IncubeStop,
            RoomStart,
            RoomStop
        }
    }
}