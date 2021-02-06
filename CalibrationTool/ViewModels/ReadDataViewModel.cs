using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonLib.Mvvm;
using CalibrationTool.Models;
using CalibrationTool.Utils;
using System.Reflection;

namespace CalibrationTool.ViewModels
{
    public class ReadDataViewModel : BindableBase
    {
        #region Command
        public ICommand SendDebugCommand { get; set; }
        public ICommand SendCaliCommand { get; set; }
        public ICommand SendReadFlowCommand { get; private set; }
        public ICommand SendAVStartCommand { get; set; }
        public ICommand SendAVStopCommand { get; set; }
        public ICommand SendCheckAVStartCommand { get; set; }
        public ICommand SendCheckStopCommand { get; set; }
        public ICommand SendAIStartCommand { get; set; }
        public ICommand SendAIStopCommand { get; set; }
        public ICommand SendCheckAIStartCommand { get; set; }
        public ICommand SendPWMTestStartCommand { get; set; }
        public ICommand SendPWMTestStopCommand { get; set; }
        #endregion

        #region Properties
        public string DebugCommand { get => ConfigManager.DebugCommand; }
        public string CaliCommand { get => ConfigManager.CaliFlowVCommand; }
        public string AVStopCommand { get => ConfigManager.AVStopCommand; }
        public string AIStopCommand { get => ConfigManager.AIStopCommand; }
        public string CheckStopCommand { get => ConfigManager.CheckStopCommand; }
        public string PWMTestStartCommand { get => ConfigManager.PWMTestStartCommand; }
        public string PWMTestStopCommand { get => ConfigManager.PWMTestStopCommand; }
        public byte[] FlowCommand { get => ConfigManager.ReadFlowCommand; }

        public DebugData DebugData { get; private set; } = new DebugData();

        private bool _repeat;
        public bool Repeat
        {
            get => _repeat;
            set => SetProperty(ref _repeat, value);
        }

        private int _interval = 100;
        public int Interval
        {
            get => _interval;
            set => SetProperty(ref _interval, value);
        }

        private float _avStartValue;
        public float AVStartValue
        {
            get => _avStartValue;
            set => SetProperty(ref _avStartValue, value);
        }

        private float _aiStartValue;
        public float AIStartValue
        {
            get => _aiStartValue;
            set => SetProperty(ref _aiStartValue, value);
        }

        private float _checkAVValue;
        public float CheckAVValue
        {
            get => _checkAVValue;
            set => SetProperty(ref _checkAVValue, value);
        }

        private float _checkAVKValue;
        public float CheckAVKValue
        {
            get => _checkAVKValue;
            set => SetProperty(ref _checkAVKValue, value);
        }

        private float _checkAVDValue;
        public float CheckAVDValue
        {
            get => _checkAVDValue;
            set => SetProperty(ref _checkAVDValue, value);
        }

        private float _checkAIValue;
        public float CheckAIValue
        {
            get => _checkAIValue;
            set => SetProperty(ref _checkAIValue, value);
        }

        private float _checkAIKValue;
        public float CheckAIKValue 
        { 
            get => _checkAIKValue;
            set => SetProperty(ref _checkAIKValue, value);
        }

        private float _checkAIDValue;
        public float CheckAIDValue
        {
            get => _checkAIDValue;
            set => SetProperty(ref _checkAIDValue, value);
        }
        #endregion

        #region Public Methods
        public void SetDebugData(KeyValuePair<string, string> keyValue)
        {
            PropertyInfo property = DebugDataMapperAttribute.GetPropertyByKey(keyValue.Key);
            if (property == null || !property.CanWrite) return;
            if (DebugData.TryToSetPropertyValue(property, keyValue.Value))
                RaiseProperty(nameof(DebugData));
        }

        public void SetReadFlowCommand(Action act)
        {
            SendReadFlowCommand = new RelayCommand(
                o => Task.Run(async () =>
                {
                    if (_repeat && _interval <= 0) return;
                    do
                    {
                        act?.Invoke();
                        await Task.Delay(_interval);
                    } while (_repeat);
                })
            );
        }

        public string GetAVStartCommand() => 
            string.Format("{0}:{1}!", ConfigManager.AVStartCommandHeader, _avStartValue);

        public string GetAIStartCommand() =>
            string.Format("{0}:{1}!", ConfigManager.AIStartCommandHeader, _aiStartValue);

        public string GetCheckAVStartCommand() =>
            string.Format("{0}:{1};{2};{3}!", ConfigManager.CheckAVStartCommandHeader,
                _checkAVValue, _checkAVKValue, _checkAVDValue);

        public string GetCheckAIStartCommand() =>
            string.Format("{0}:{1};{2};{3}!", ConfigManager.CheckAIStartCommandHeader,
                _checkAIValue, _checkAIKValue, _checkAIDValue);
        #endregion

        #region Private Methods

        #endregion
    }
}
