using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

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

        public string AppTitle { get; set; }

        public SerialPortInstance Instance { get; }
        public ObservableCollection<string> LatestCommands { get; }
        public string SelectedLatestCommand
        {
            get => selectedLatestCommand;
            set
            {
                _ = Set(ref selectedLatestCommand, value);
                EditableCommand = value;
            }
        }
        public string EditableCommand
        {
            get => editableCommand;
            set => _ = Set(ref editableCommand, value);
        }

        public RelayCommand SendCommand => new(Send);

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
                using FileStream stream = new(file, FileMode.Append, FileAccess.Write);
                using StreamWriter writer = new(stream);
                await writer.WriteLineAsync(command);
            }
        }

        private async void InitializeLatestCommands()
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"./commands.txt");
            using FileStream stream = new(file, FileMode.OpenOrCreate, FileAccess.Read);
            using StreamReader reader = new(stream);
            string line;
            List<string> commands = new();
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    commands.Add(line);
                }
            }
            var orderComamnds = from c in commands
                                group c by c into g
                                select (command: g.Key, count: g.Count()) into cc
                                orderby cc.count descending
                                select cc.command;
            foreach (string command in orderComamnds.Take(10))
            {
                LatestCommands.Add(command);
            }
        }

        public void SetAppTitleForConfig(string configFileName)
        {
            StringBuilder builder = new();
            builder.Append(configFileName.Split('.')[0]);
            builder.Append(" - AutoCommander ");
            var version = Application.ResourceAssembly.GetName().Version;
            builder.Append("v").Append(version);
            AppTitle = builder.ToString();
            RaisePropertyChanged(nameof(AppTitle));
        }
    }
}