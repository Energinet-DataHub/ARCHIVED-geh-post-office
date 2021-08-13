using System;
using Microsoft.Azure.Functions.Worker;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common.SimpleInjector
{
    public class SimpleInjectorActivator : IFunctionActivator
    {
        private readonly Container _container;

        public SimpleInjectorActivator(Container container)
        {
            _container = container;
        }

        public object? CreateInstance(Type instanceType, FunctionContext context)
        {
            if (instanceType == null) throw new ArgumentNullException(nameof(instanceType));
            return _container.GetInstance(instanceType);
        }
    }
}
