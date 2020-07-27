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
        public ICommand SendRefStartCommand { get; set; }
        public ICommand SendRefStopCommand { get; set; }
        public ICommand SendCheckCommand { get; set; }
        public ICommand SendCheckStopCommand { get; set; }
        #endregion

        #region Properties
        public string DebugCommand { get => ConfigManager.DebugCommand; }
        public string CaliCommand { get => ConfigManager.CaliFlowVCommand; }
        public string RefStopCommand { get => ConfigManager.AVStopCommand; }
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

        private float _refStartValue;
        public float RefStartValue
        {
            get => _refStartValue;
            set => SetProperty(ref _refStartValue, value);
        }

        private float _checkVoltValue;
        public float CheckVoltValue
        {
            get => _checkVoltValue;
            set => SetProperty(ref _checkVoltValue, value);
        }

        private float _checkKValue;
        public float CheckKValue
        {
            get => _checkKValue;
            set => SetProperty(ref _checkKValue, value);
        }

        private float _checkDValue;
        public float CheckDValue
        {
            get => _checkDValue;
            set => SetProperty(ref _checkDValue, value);
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

        public string GetRefStartCommand() => 
            string.Format("{0}:{1}!", ConfigManager.AVStartCommandHeader, _refStartValue);

        public string GetCheckCommand() =>
            string.Format("{0}:{1};{2};{3}!", ConfigManager.CheckCommandHeader,
                _checkVoltValue, _checkKValue, _checkDValue);
        #endregion

        #region Private Methods

        #endregion
    }
}
