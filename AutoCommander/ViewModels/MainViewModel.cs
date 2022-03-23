using AutoCommander.Common;
using AutoCommander.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoCommander.ViewModels;

public class MainViewModel : ObservableObject
{
    #region ctor
    public MainViewModel()
    {
        LatestCommands = new ObservableCollection<string>();
        InitializeLatestCommands();
    }
    #endregion

    #region fields
    private string selectedLatestCommand;
    private string editableCommand;
    private DataType sendDataType = DataType.ASCII;
    private DataType receiveDataType = DataType.ASCII;
    private bool autoSendingNewLine = false;
    private bool autoReceivingNewLine = true;
    #endregion

    #region properties
    public string AppTitle { get; private set; }

    public ObservableCollection<string> LatestCommands { get; }

    public string SelectedLatestCommand
    {
        get => selectedLatestCommand;
        set
        {
            _ = SetProperty(ref selectedLatestCommand, value);
            EditableCommand = value;
        }
    }

    public string EditableCommand
    {
        get => editableCommand;
        set => SetProperty(ref editableCommand, value);
    }

    public DataType SendDataType
    {
        get => sendDataType;
        set => SetProperty(ref sendDataType, value);
    }

    public DataType ReceiveDataType
    {
        get => receiveDataType;
        set => SetProperty(ref receiveDataType, value);
    }

    public bool AutoSendingNewLine
    {
        get => autoSendingNewLine;
        set => SetProperty(ref autoSendingNewLine, value);
    }

    public bool AutoReceivingNewLine
    {
        get => autoReceivingNewLine;
        set => SetProperty(ref autoReceivingNewLine, value);
    }

    public string AppStatus { get; private set; }
    #endregion

    #region public methods
    public void SetAppStatus(string status)
    {
        AppStatus = status;
        OnPropertyChanged(nameof(AppStatus));
    }

    public void ClearAppStatus()
    {
        AppStatus = string.Empty;
        OnPropertyChanged(nameof(AppStatus));
    }

    public void SetAppTitleForConfig(string configFileName)
    {
        StringBuilder builder = new();
        builder.Append(configFileName.Split('.')[0]);
        builder.Append(" - AutoCommander ");
        var version = Application.ResourceAssembly.GetName().Version;
        builder.Append("v").Append(version);
        AppTitle = builder.ToString();
        OnPropertyChanged(nameof(AppTitle));
    }

    public async ValueTask WriteLatestCommand(string command)
    {
        string file = PathUtils.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.LatestCommandFile);
        using FileStream stream = new(file, FileMode.Append, FileAccess.Write);
        using StreamWriter writer = new(stream);
        await writer.WriteLineAsync(command);
    }
    #endregion

    #region private methods
    private async ValueTask<List<string>> ReadLatestCommandFileAsync()
    {
        string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.LatestCommandFile);
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
        return commands;
    }

    private async void InitializeLatestCommands()
    {
        var commands = await ReadLatestCommandFileAsync();
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

    #endregion
}
