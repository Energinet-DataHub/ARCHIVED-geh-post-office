using System;
using Energinet.DataHub.PostOffice.Infrastructure;
using GreenEnergyHub.PostOffice.Communicator.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Container = SimpleInjector.Container;

namespace Energinet.DataHub.PostOffice.Common
{
    internal static class ServiceBusRegistration
    {
        public static void AddServiceBus(this Container container)
        {
            container.RegisterSingleton<IServiceBusClientFactory>(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var connectionString = configuration.GetConnectionStringOrSetting("ServiceBusConnectionString");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid ServiceBus in the appSettings.json file or your Azure Functions Settings.");
                }

                return new ServiceBusClientFactory(connectionString);
            });
        }

        public static void AddServiceBusConfig(this Container container)
        {
            container.RegisterSingleton(() =>
            {
                var configuration = container.GetService<IConfiguration>();

                return new ServiceBusConfig(
                    configuration.GetValue<string>(ServiceBusConfig.DataAvailableQueueNameKey),
                    configuration.GetValue<string>(ServiceBusConfig.DataAvailableQueueConnectionStringKey));
            });
        }
    }
}
