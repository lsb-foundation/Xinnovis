using CommonLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CalibrationTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private StringBuilder builder = new StringBuilder();
        public string ReceivedData
        {
            get => builder.ToString();
        }

        private string _codeToSend;
        public string CodeToSend
        {
            get => _codeToSend;
            set => SetProperty(ref _codeToSend, value);
        }

        public RelayCommand ClearDisplayCommand { get; set; }
        public RelayCommand CopyDisplayContentCommand { get; set; }

        public MainWindowViewModel()
        {
            ClearDisplayCommand = new RelayCommand(obj => ClearDisplay());
            CopyDisplayContentCommand = new RelayCommand(obj => CopyDisplayContent());
        }

        public void AppendStringToBuilder(string data)
        {
            builder.Append(data);
            RaiseProperty(nameof(ReceivedData));
        }

        private void ClearDisplay()
        {
            builder.Clear();
            RaiseProperty(nameof(ReceivedData));
        }

        private void CopyDisplayContent()
        {
            Clipboard.SetText(builder.ToString());
        }
    }
}
