using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace AutoCommander.ViewModels;

public class ViewModelLocator
{
    public MainViewModel Main => Ioc.Default.GetService<MainViewModel>();
    public SerialPortViewModel Serial => Ioc.Default.GetService<SerialPortViewModel>();
    public ConfigurationViewModel Configuration => Ioc.Default.GetService<ConfigurationViewModel>();
}
