using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace AutoCommander.ViewModels;

public class ViewModelLocator
{
    public ViewModelLocator()
    {
        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddSingleton<MainViewModel>()
            .AddSingleton<SerialPortViewModel>()
            .AddSingleton<ConfigurationViewModel>()
            .BuildServiceProvider());
    }

    public MainViewModel Main => Ioc.Default.GetService<MainViewModel>();
    public SerialPortViewModel Serial => Ioc.Default.GetService<SerialPortViewModel>();
    public ConfigurationViewModel Configuration => Ioc.Default.GetService<ConfigurationViewModel>();
}
