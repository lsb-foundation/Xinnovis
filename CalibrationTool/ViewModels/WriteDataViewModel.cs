﻿using CommonLib.Mvvm;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.Windows.Input;
using System.Collections.ObjectModel;
using CalibrationTool.Models;
using Microsoft.Win32;

namespace CalibrationTool.ViewModels
{
    public class WriteDataViewModel : BindableBase
    {
        #region feild
        private readonly List<float> voltValues = new List<float>(17);
        private readonly List<float> kValues = new List<float>(16);
        #endregion

        #region Command
        public ICommand ReadExcelCommand { get; private set; }
        public ICommand SendVoltCommand { get; set; }
        public ICommand SendKCommand { get; set; }
        public ICommand SetGasCommand { get; set; }
        public Action<string> MessageHandler { get; set; }
        #endregion

        #region Properties
        private string _voltCommand;
        public string VoltCommand
        {
            get => _voltCommand;
            set => SetProperty(ref _voltCommand, value);
        }

        private string _kCommand;
        public string KCommand
        {
            get => _kCommand;
            set => SetProperty(ref _kCommand, value);
        }

        public ObservableCollection<GasTypeCode> GasTypeCodeCollection { get; private set; }
        public ObservableCollection<UnitCode> UnitCodeCollection { get; private set; }
        private GasTypeCode _selectedGasTypeCode;
        public GasTypeCode SelectedGasTypeCode
        {
            get => _selectedGasTypeCode;
            set => SetProperty(ref _selectedGasTypeCode, value);
        }
        private UnitCode _selectedUnitCode;
        public UnitCode SelectedUnitCode
        {
            get => _selectedUnitCode;
            set => SetProperty(ref _selectedUnitCode, value);
        }

        private int _range;
        public int Range
        {
            get => _range;
            set => SetProperty(ref _range, value);
        }
        #endregion

        #region Constructed functions
        public WriteDataViewModel()
        {
            InitializeCollections();
            ReadExcelCommand = new RelayCommand(o => OpenExcelFile());
        }
        #endregion

        #region Public methods

        #endregion

        #region Private methods
        private void InitializeCollections()
        {
            GasTypeCodeCollection = new ObservableCollection<GasTypeCode>();
            UnitCodeCollection = new ObservableCollection<UnitCode>();
            GasTypeCode.GetGasTypeCodes().ForEach(gtc => GasTypeCodeCollection.Add(gtc));
            UnitCode.GetUnitCodes().ForEach(uc => UnitCodeCollection.Add(uc));
            SelectedGasTypeCode = GasTypeCodeCollection[0];
            SelectedUnitCode = UnitCodeCollection[0];
        }

        private void OpenExcelFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Excel文件|*.xls;*.xlsx";
            dialog.Title = "打开Excel文件";
            dialog.Multiselect = false;
            if((bool)dialog.ShowDialog())
            {
                if (dialog.CheckFileExists)
                {
                    ReadExcelFile(dialog.FileName);
                }
            }
        }

        private void ReadExcelFile(string file)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var stream = File.OpenRead(file))
                using (var package = new ExcelPackage(stream))
                using (var sheet = package.Workbook.Worksheets[0])
                {
                    voltValues.Clear();
                    kValues.Clear();
                    for(int row = 1; row <= 17; row++)
                    {
                        string rowStr = (row + 1).ToString();
                        string voltCell = "B" + rowStr;
                        string kCell = "C" + rowStr;
                        float voltValue = Convert.ToSingle(sheet.Cells[voltCell].Value);
                        voltValues.Add(voltValue);
                        if(row <= 16)
                        {
                            float kValue = Convert.ToSingle(sheet.Cells[kCell].Value);
                            kValues.Add(kValue);
                        }
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

        private string GetGasCommand() =>
            string.Format("GAS:{0};{1};{2}!",
                _selectedGasTypeCode.Code, _range, _selectedUnitCode.Code);

        private string GetVoltCommand()
        {
            string command = "VOLT:";
            voltValues.ForEach(v => command += v.ToString() + ";");
            return command.Substring(0, command.Length - 1) + "!";
        }

        private string GetKCommand()
        {
            string command = "K:";
            kValues.ForEach(k => command += k.ToString() + ";");
            return command.Substring(0, command.Length - 1) + "!";
        }
        #endregion
    }
}
