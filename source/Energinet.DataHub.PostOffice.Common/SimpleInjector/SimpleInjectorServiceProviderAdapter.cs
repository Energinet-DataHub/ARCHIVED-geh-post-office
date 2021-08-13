using System;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common.SimpleInjector
{
    public class SimpleInjectorServiceProviderAdapter : IServiceProvider
    {
        private readonly Container _container;

        public SimpleInjectorServiceProviderAdapter(Container container)
        {
            _container = container;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            return _container.GetInstance(serviceType);
        }
    }
}
