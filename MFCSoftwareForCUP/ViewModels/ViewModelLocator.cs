using Autofac;

namespace MFCSoftwareForCUP.ViewModels
{
    public class ViewModelLocator
    {
        private readonly IContainer _container;

        public ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new MainViewModel()).SingleInstance().AsSelf();
            builder.Register(context => new ChannelViewModel()).AsSelf();
            _container = builder.Build();
        }

        public MainViewModel Main => _container.Resolve<MainViewModel>();
        public ChannelViewModel Channel => _container.Resolve<ChannelViewModel>();
    }
}
