using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace AwesomeCommand.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string selectedLatestCommand;
        private string editableCommand;

        public MainViewModel(SerialPortInstance instance)
        {
            Instance = instance;
            LatestCommands = new ObservableCollection<string>();
        }

        public SerialPortInstance Instance { get; }
        public ObservableCollection<string> LatestCommands { get; }
        public string SelectedLatestCommand
        {
            get => selectedLatestCommand;
            set
            {
                _ = Set(ref selectedLatestCommand, value);
                _ = Set(nameof(EditableCommand), ref editableCommand, value);
            }
        }
        public string EditableCommand
        {
            get => editableCommand;
            set => _ = Set(ref editableCommand, value);
        }
    }
}