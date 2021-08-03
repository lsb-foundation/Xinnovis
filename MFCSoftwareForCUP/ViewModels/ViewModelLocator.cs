using Autofac;

namespace MFCSoftwareForCUP.ViewModels
{
    public class ViewModelLocator
    {
        private readonly IContainer _container;

        public ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            _ = builder.RegisterInstance(new MainViewModel()).SingleInstance().AsSelf();
            _ = builder.Register(context => new ChannelViewModel()).AsSelf();
            _ = builder.Register(context => new ConfirmPasswordViewModel()).AsSelf();
            _ = builder.Register(context => new ResetPasswordViewModel()).AsSelf();
            _container = builder.Build();
        }

        public MainViewModel Main => _container.Resolve<MainViewModel>();
        public ChannelViewModel Channel => _container.Resolve<ChannelViewModel>();
        public ConfirmPasswordViewModel Confirm => _container.Resolve<ConfirmPasswordViewModel>();
        public ResetPasswordViewModel Reset => _container.Resolve<ResetPasswordViewModel>();
    }
}
