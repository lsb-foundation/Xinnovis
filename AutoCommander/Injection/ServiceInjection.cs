using AutoCommander.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace AutoCommander.Injection;

public class ServiceInjection
{
    public static void ConfigureServices()
    {
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton<MainViewModel>()
                .AddSingleton<SerialPortViewModel>()
                .AddSingleton<ConfigurationViewModel>()
                .BuildServiceProvider());
    }
}
