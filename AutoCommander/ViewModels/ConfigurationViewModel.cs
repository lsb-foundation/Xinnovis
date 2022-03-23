using AutoCommander.Properties;
using AutoCommander.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using AutoCommander.AutoUI.Linkers;

namespace AutoCommander.ViewModels;

public class ConfigurationViewModel : ObservableObject
{
    public ConfigurationViewModel()
    {
        LoadSettings();
    }

    public List<string> AutoUiFiles
    {
        get
        {
            string path = PathUtils.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.AutoUIFolder);
            string[] files = Directory.GetFiles(path);

            return files
                .Select(f => new FileInfo(f))
                .Where(f => f.Extension.ToLower() == ".xml")
                .OrderBy(f => f.Name)
                .Select(f => f.Name)
                .ToList();
        }
    }

    private string selectedFile;
    public string SelectedAutoUiFile
    {
        get => selectedFile;
        set => SetProperty(ref selectedFile, value);
    }

    private Linker linker;
    public List<LinkerItem> LinkerItems => linker?.Items;

    public void SetLinker(Linker linker)
    {
        this.linker = linker;
        OnPropertyChanged(nameof(LinkerItems));
    }

    private void LoadSettings()
    {
        string file = Settings.Default.AutoUiFile;
        if (AutoUiFiles.Contains(file))
        {
            SelectedAutoUiFile = file;
        }
    }
}
