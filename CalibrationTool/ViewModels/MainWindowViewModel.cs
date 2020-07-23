using CalibrationTool.Utils;
using CommonLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CalibrationTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _codeToSend;
        public string CodeToSend
        {
            get => _codeToSend;
            set => SetProperty(ref _codeToSend, value);
        }

        public string AppTitle { get => ConfigManager.AppTitle; }

        public ICommand ClearDisplayCommand { get; set; }
        public ICommand CopyDisplayContentCommand { get; set; }
        public ICommand SendCommand { get; set; }
        public Action<string> AppendTextToDisplayAction { get; set; }

        public void AppendTextToDisplay(string text)
        {
            AppendTextToDisplayAction?.Invoke(text);
        }
    }
}
