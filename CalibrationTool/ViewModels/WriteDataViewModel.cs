using GalaSoft.MvvmLight;
using System;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using CalibrationTool.Utils;
using CommonLib.Models;
using GalaSoft.MvvmLight.Command;

namespace CalibrationTool.ViewModels
{
    public class WriteDataViewModel : ViewModelBase, IMessageHandler<string>
    {
        #region feild
        private readonly List<float> voltValues;
        private readonly List<float> kValues;
        #endregion

        #region Command
        public ICommand ReadExcelCommand { get; private set; }
        public ICommand SendVoltCommand { get; set; }
        public ICommand SendKCommand { get; set; }
        public ICommand SetGasCommand { get; set; }
        public ICommand SetTemperatureCommand { get; set; }
        public ICommand SetAvCommand { get; set; }
        public ICommand SetAiCommand { get; set; }
        public ICommand SetPWMCommand { get; set; }
        public ICommand SendClearEEPRomCommand { get; set; }
        public ICommand SetGasFactorCommand { get; set; }
        public Action<string> MessageHandler { get; set; }
        #endregion

        #region Properties
        private string _voltCommand;
        public string VoltCommand
        {
            get => _voltCommand;
            set => Set(ref _voltCommand, value);
        }

        private string _kCommand;
        public string KCommand
        {
            get => _kCommand;
            set => Set(ref _kCommand, value);
        }

        public string ClearEEPRomCommand { get => ConfigManager.ClearEEPRomCommand; }

        public ObservableCollection<GasTypeCode> GasTypeCodeCollection { get; private set; }
        public ObservableCollection<UnitCode> UnitCodeCollection { get; private set; }

        private GasTypeCode _selectedGasTypeCode;
        public GasTypeCode SelectedGasTypeCode
        {
            get => _selectedGasTypeCode;
            set => Set(ref _selectedGasTypeCode, value);
        }

        private UnitCode _selectedUnitCode;
        public UnitCode SelectedUnitCode
        {
            get => _selectedUnitCode;
            set => Set(ref _selectedUnitCode, value);
        }

        private float _temperature;
        public float Temperature
        {
            get => _temperature;
            set => Set(ref _temperature, value);
        }

        private float _gasFactor;
        public float GasFactor
        {
            get => _gasFactor;
            set => Set(ref _gasFactor, value);
        }

        private float _range;
        public float Range
        {
            get => _range;
            set => Set(ref _range, value);
        }

        private float _avKValue;
        public float AvKValue
        {
            get => _avKValue;
            set => Set(ref _avKValue, value);
        }

        private float _avDValue;
        public float AvDValue
        {
            get => _avDValue;
            set => Set(ref _avDValue, value);
        }

        private float _aiKValue;
        public float AiKValue
        {
            get => _aiKValue;
            set => Set(ref _aiKValue, value);
        }

        private float _aiDValue;
        public float AiDValue
        {
            get => _aiDValue;
            set => Set(ref _aiDValue, value);
        }

        private float _pid_P;
        public float Pid_P
        {
            get => _pid_P;
            set => Set(ref _pid_P, value);
        }

        private float _pid_i;
        public float Pid_I
        {
            get => _pid_i;
            set => Set(ref _pid_i, value);
        }

        private float _pid_d;
        public float Pid_D
        {
            get => _pid_d;
            set => Set(ref _pid_d, value);
        }

        private int _pid_zero;
        public int Pid_Zero
        {
            get => _pid_zero;
            set => Set(ref _pid_zero, value);
        }
        #endregion

        #region Constructed functions
        public WriteDataViewModel()
        {
            InitializeCollections();

            int voltDataLength = ConfigManager.VoltDataLength;
            int kDataLength = ConfigManager.KDataLength;
            voltValues = new List<float>(voltDataLength);
            kValues = new List<float>(kDataLength);

            ReadExcelCommand = new RelayCommand(() => OpenExcelFile());
        }
        #endregion

        #region Public methods
        public string GetGasCommand() =>
            string.Format("{0}:{1};{2};{3}!",
                ConfigManager.GasCommandHeader, _selectedGasTypeCode.Code,
                _range, _selectedUnitCode.Code);

        public string GetTemperatureCommand() =>
            string.Format("{0}:{1}!", ConfigManager.CaliTCommandHeader, _temperature);

        public string GetAvCommand() =>
            string.Format("{0}:{1};{2}!", ConfigManager.AVCommandHeader, _avKValue, _avDValue);

        public string GetAiCommand() =>
            string.Format("{0}:{1};{2}!", ConfigManager.AICommandHeader, _aiKValue, _aiDValue);

        public string GetPWMCommand() =>
            string.Format("{0}:{1};{2};{3};{4}!", ConfigManager.PWMCommandHeader, _pid_P, _pid_i, _pid_d, _pid_zero);

        public string GetGasFactorCommand() =>
            string.Format("{0}:{1}!", ConfigManager.GasFactorCommandHeader, _gasFactor);
        #endregion

        #region Private methods
        private void InitializeCollections()
        {
            GasTypeCodeCollection = new ObservableCollection<GasTypeCode>();
            UnitCodeCollection = new ObservableCollection<UnitCode>();
            GasTypeCode.GetGasTypeCodesFromConfiguration()?.ForEach(gtc => GasTypeCodeCollection.Add(gtc));
            UnitCode.GetUnitCodesFromConfiguration()?.ForEach(uc => UnitCodeCollection.Add(uc));
            SelectedGasTypeCode = GasTypeCodeCollection[0];
            SelectedUnitCode = UnitCodeCollection[0];
        }

        private void OpenExcelFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel文件|*.xls;*.xlsx",
                Title = "打开Excel文件",
                Multiselect = false
            };
            if ((bool)dialog.ShowDialog())
            {
                ReadExcelFile(dialog.FileName);
            }
        }

        private void ReadExcelFile(string file)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var package = new ExcelPackage(stream))
                using (var sheet = package.Workbook.Worksheets[0])
                {
                    voltValues.Clear();
                    kValues.Clear();
                    for (int row = 1; row <= voltValues.Capacity; row++)
                    {
                        string rowStr = (row + 1).ToString();
                        string voltCell = "B" + rowStr;
                        float voltValue = Convert.ToSingle(sheet.Cells[voltCell].Value);
                        voltValues.Add(voltValue);
                    }
                    for (int row = 1; row <= kValues.Capacity; row++)
                    {
                        string rowStr = (row + 1).ToString();
                        string kCell = "C" + rowStr;
                        float kValue = Convert.ToSingle(sheet.Cells[kCell].Value);
                        kValues.Add(kValue);
                    }
                    VoltCommand = GetVoltCommand();
                    KCommand = GetKCommand();
                }
            }
            catch(Exception e)
            {
                MessageHandler?.Invoke("读取Excel文件出错：" + e.Message);
            }
        }

        private string GetVoltCommand()
        {
            string command = ConfigManager.CaliVoltCommandHeader + ":";
            voltValues.ForEach(v => command += v.ToString() + ";");
            return command.Substring(0, command.Length - 1) + "!";
        }

        private string GetKCommand()
        {
            string command = ConfigManager.CaliKCommandHeader + ":";
            kValues.ForEach(k => command += k.ToString() + ";");
            return command.Substring(0, command.Length - 1) + "!";
        }
        #endregion
    }
}
