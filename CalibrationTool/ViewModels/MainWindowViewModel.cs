using CalibrationTool.Utils;
using GalaSoft.MvvmLight;
using System;
using System.Windows.Input;

namespace CalibrationTool.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _codeToSend;
        public string CodeToSend
        {
            get => _codeToSend;
            set => Set(ref _codeToSend, value);
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
