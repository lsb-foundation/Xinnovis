using AutoCalibrationTool.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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
            TestLeakageOnCommand = new RelayCommand(() => Send("TEST_LEAKAGE_ON!", CommandType.TestLeakageOn));
            TestLeakageOffCommand = new RelayCommand(() => Send("TEST_LEAKAGE_OFF!", CommandType.TestLeakageOff));
            SendCommand = new RelayCommand(() =>
            {
                if (!string.IsNullOrWhiteSpace(command))
                {
                    string currentCommand = command.Trim();
                    Send(currentCommand, CommandType.CustomCommand);
                    WriteCommandToHistoryFile(currentCommand);
                    if (!HistoryCommands.Contains(currentCommand))
                    {
                        HistoryCommands.Insert(0, currentCommand);
                    }
                }
            });
            GetHistoryCommandsTop10();
        }

        #region Fields
        private const string historyCommandsFile = @"Records\commands.txt";
        private string command;
        private string selectedCommand;
        #endregion

        #region Properties
        public bool CommandButtonEnabled => ViewModelLocator.Port.IsOpen && Mode == CalibrationMode.Stop;
        public bool IncubeStopButtonEnabled => ViewModelLocator.Port.IsOpen && Mode == CalibrationMode.Incube;
        public bool RoomStopButtonEnabled => ViewModelLocator.Port.IsOpen && Mode == CalibrationMode.Room;
        public bool TestLeakageOffButtonEnabled => ViewModelLocator.Port.IsOpen && Mode == CalibrationMode.TestLeakage;
        public CalibrationMode Mode { get; private set; } = CalibrationMode.Stop;
        public int DeviceDataCount { get; private set; }
        public int FlowDataCount { get; private set; }
        public string Command
        {
            get => command;
            set => Set(ref command, value);
        }
        public ObservableCollection<string> HistoryCommands { get; set; } = new ObservableCollection<string>();
        public string SelectedCommand
        {
            get => selectedCommand;
            set
            {
                Set(ref selectedCommand, value);
                Command = value;
            }
        }
        #endregion

        #region Commands
        public RelayCommand IncubeStartCommand { get; }
        public RelayCommand IncubeStopCommand { get; }
        public RelayCommand RoomStartCommand { get; }
        public RelayCommand RoomStopCommand { get; }
        public RelayCommand TestLeakageOnCommand { get; }
        public RelayCommand TestLeakageOffCommand { get; }
        public RelayCommand SendCommand { get; }
        #endregion

        #region Methods
        public void UpdateButtonEnableStatus()
        {
            RaisePropertyChanged(nameof(CommandButtonEnabled));
            RaisePropertyChanged(nameof(IncubeStopButtonEnabled));
            RaisePropertyChanged(nameof(RoomStopButtonEnabled));
            RaisePropertyChanged(nameof(TestLeakageOffButtonEnabled));
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
                case CommandType.RoomStart:
                    Mode = CalibrationMode.Room;
                    break;
                case CommandType.TestLeakageOn:
                    Mode = CalibrationMode.TestLeakage;
                    break;
                default:
                    Mode = CalibrationMode.Stop;
                    break;
            }
            UpdateButtonEnableStatus();
            ViewModelLocator.Port.UpdatePortButtonStatus();
            ViewModelLocator.Storage.UpdateButtonStatus();
        }

        private async void GetHistoryCommandsTop10()
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, historyCommandsFile);
            CreateDirectoryForFile(file);
            using (var stream = new FileStream(file,FileMode.OpenOrCreate, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                List<string> lines = new List<string>();
                string line = null;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }

                //Takes top 10 commands by using frequency
                var top10 = lines.GroupBy(ln => ln)
                                 .Select(g => (g.Key, Count: g.Count()))
                                 .OrderByDescending(kc => kc.Count)
                                 .ThenBy(kc => kc.Key)
                                 .Take(10);

                foreach ((string key, int count) in top10)
                {
                    HistoryCommands.Add(key);
                }
                if (HistoryCommands.Count > 0)
                {
                    SelectedCommand = HistoryCommands[0];
                }
            }
        }

        private async void WriteCommandToHistoryFile(string command)
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, historyCommandsFile);
            CreateDirectoryForFile(file);
            using (var stream = new FileStream(file, FileMode.Append, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync(command);
            }
        }

        private void CreateDirectoryForFile(string file)
        {
            var directory = Path.GetDirectoryName(file);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        #endregion

        enum CommandType
        {
            IncubeStart,
            IncubeStop,
            RoomStart,
            RoomStop,
            TestLeakageOn,
            TestLeakageOff,
            CustomCommand
        }
    }
}