using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace MFCSoftwareForCUP.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            var services = new ServiceCollection()
                .AddSingleton<MainViewModel>()
                .AddTransient<ChannelViewModel>()
                .AddTransient<ConfirmPasswordViewModel>()
                .AddTransient<ResetPasswordViewModel>();

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }

        public MainViewModel Main => Ioc.Default.GetService<MainViewModel>();
        public ChannelViewModel Channel => Ioc.Default.GetService<ChannelViewModel>();
        public ConfirmPasswordViewModel Confirm => Ioc.Default.GetService<ConfirmPasswordViewModel>();
        public ResetPasswordViewModel Reset => Ioc.Default.GetService<ResetPasswordViewModel>();
    }
}
