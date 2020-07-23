using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonLib.Mvvm;
using CalibrationTool.Models;

namespace CalibrationTool.ViewModels
{
    public class ReadDataViewModel : BindableBase
    {
        #region Command
        public ICommand SendDebugCommand { get; set; }
        public ICommand SendCaliCommand { get; set; }
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
        public string DebugCommand { get => "DEBUG!"; }
        public string CaliCommand { get => "CALI!"; }
        public string RefStopCommand { get => "REF_STOP!"; }
        public byte[] FlowCommand { get => new byte[1] { 0x90 }; }

        private DebugData _debugData;
        public string SN { get => _debugData.SN; }
        public string GasType { get => _debugData.GasType; }
        public string Range { get => _debugData.Range; }
        public string Unit { get => _debugData.Unit; }

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

        public string GetRefStartCommand() => 
            string.Format("REF_START:{0}!", _refStartValue);
        #endregion

        #region Private Methods

        #endregion
    }
}
