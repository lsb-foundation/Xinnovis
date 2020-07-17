using CommonLib.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalibrationTool.Models;

namespace CalibrationTool.ViewModels
{
    public class DebugViewModel : BindableBase
    {
        private DebugData _data;

        public string SN
        {
            get => _data.SN;
        }

        public string GasType
        {
            get => _data.GasType;
        }

        public string Range
        {
            get => _data.Range;
        }

        public string Unit
        {
            get => _data.Unit;
        }

        public RelayCommand DebugCommand { get; set; }

        public DebugViewModel()
        {
            _data = new DebugData();
        }

        public void SetDataProperty(KeyValuePair<string, string> keyValue)
        {
            switch (keyValue.Key)
            {
                case "SN":
                    _data.SN = keyValue.Value;
                    RaiseProperty(nameof(SN));
                    break;
                case "Type of GAS":
                    _data.GasType = keyValue.Value;
                    RaiseProperty(nameof(GasType));
                    break;
                case "Range":
                    _data.Range = keyValue.Value;
                    RaiseProperty(nameof(Range));
                    break;
                case "UNIT":
                    _data.Unit = keyValue.Value;
                    RaiseProperty(nameof(Unit));
                    break;
                default:
                    return;
            }
        }
    }
}
