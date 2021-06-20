using Autofac;

namespace AutoCalibrationTool.ViewModel
{
    public class ViewModelLocator
    {
        private static readonly IContainer _container;
        static ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new MainViewModel()).AsSelf().ExternallyOwned();
            builder.RegisterInstance(new PortViewModel()).AsSelf().ExternallyOwned();
            builder.RegisterInstance(new StorageViewModel()).AsSelf().ExternallyOwned();
            _container = builder.Build();
        }

        public static MainViewModel Main => _container.Resolve<MainViewModel>();
        public static PortViewModel Port => _container.Resolve<PortViewModel>();
        public static StorageViewModel Storage => _container.Resolve<StorageViewModel>();
        
        public static void Cleanup()
        {
            _container.Dispose();
        }
    }
}