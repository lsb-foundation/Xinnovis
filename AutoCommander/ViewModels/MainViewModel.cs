using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AutoCommander.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string selectedLatestCommand;
        private string editableCommand;

        public MainViewModel(SerialPortInstance instance)
        {
            Instance = instance;
            LatestCommands = new ObservableCollection<string>();
            InitializeLatestCommands();
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

        public RelayCommand SendCommand => new RelayCommand(Send);

        private async void Send()
        {
            string command = EditableCommand?.Trim();
            if (!string.IsNullOrWhiteSpace(command))
            {
                Instance.Send(command);
                if (!LatestCommands.Contains(command))
                {
                    LatestCommands.Insert(0, command);
                }
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"./commands.txt");
                using (FileStream stream = new FileStream(file, FileMode.Append, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    await writer.WriteLineAsync(command);
                }
            }
        }

        private async void InitializeLatestCommands()
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"./commands.txt");
            using (FileStream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                List<string> commands = new List<string>();
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        commands.Add(line);
                    }
                }
                IEnumerable<string> orderComamnds = from c in commands
                                                    group c by c into g
                                                    select (command: g.Key, count: g.Count()) into cc
                                                    orderby cc.count descending
                                                    select cc.command;
                foreach (string command in orderComamnds.Take(10))
                {
                    LatestCommands.Add(command);
                }
            }
        }
    }
}