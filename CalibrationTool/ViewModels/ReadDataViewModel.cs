using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonLib.Mvvm;
using CalibrationTool.Models;
using CalibrationTool.Utils;

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
        #endregion

        #region Constructed functions
        public ReadDataViewModel()
        {
            _debugData = new DebugData();
        }
        #endregion

        #region Properties
        public string DebugCommand { get => ConfigManager.DebugCommand; }
        public string CaliCommand { get => ConfigManager.CaliFlowVCommand; }
        public string RefStopCommand { get => ConfigManager.AVStopCommand; }
        public byte[] FlowCommand { get => ConfigManager.ReadFlowCommand; }

        private DebugData _debugData;
        public string SN { get => _debugData.SN; }
        public string GasType { get => _debugData.GasType; }
        public string Range { get => _debugData.Range; }
        public string Unit { get => _debugData.Unit; }

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
        #endregion

        #region Public Methods
        public void SetDebugData(KeyValuePair<string, string> keyValue)
        {
            switch (keyValue.Key)
            {
                case "SN":
                    _debugData.SN = keyValue.Value;
                    RaiseProperty(nameof(SN));
                    break;
                case "Type of GAS":
                    _debugData.GasType = keyValue.Value;
                    RaiseProperty(nameof(GasType));
                    break;
                case "Range":
                    _debugData.Range = keyValue.Value;
                    RaiseProperty(nameof(Range));
                    break;
                case "UNIT":
                    _debugData.Unit = keyValue.Value;
                    RaiseProperty(nameof(Unit));
                    break;
                default:
                    return;
            }
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
        #endregion

        #region Private Methods

        #endregion
    }
}
