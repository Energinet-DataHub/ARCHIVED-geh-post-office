using System;
using GreenEnergyHub.PostOffice.Communicator.DataAvailable;
using GreenEnergyHub.PostOffice.Communicator.Dequeue;
using GreenEnergyHub.PostOffice.Communicator.Factories;
using GreenEnergyHub.PostOffice.Communicator.Peek;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace GreenEnergyHub.PostOffice.Communicator.SimpleInjector
{
    public static class ContainerExtensions
    {
        public static void AddPostOfficeCommunication(this Container container, string serviceBusConnectionString)
        {
            container.AddServiceBus(serviceBusConnectionString);
            container.AddApplicationServices();
        }

        private static void AddServiceBus(this Container container, string serviceBusConnectionString)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.RegisterSingleton<IServiceBusClientFactory>(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var connectionString = configuration.GetConnectionString(serviceBusConnectionString)
                                       ?? configuration![serviceBusConnectionString];

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid ServiceBus in the appSettings.json file or your Azure Functions Settings.");
                }

                return new ServiceBusClientFactory(connectionString);
            });
        }

        private static void AddApplicationServices(this Container container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.Register<IDataAvailableNotificationSender, DataAvailableNotificationSender>(Lifestyle.Singleton);
            container.Register<IRequestBundleParser, RequestBundleParser>(Lifestyle.Singleton);
            container.Register<IResponseBundleParser, ResponseBundleParser>(Lifestyle.Singleton);
            container.Register<IDataBundleResponseSender, DataBundleResponseSender>(Lifestyle.Singleton);
            container.Register<IDequeueNotificationParser, DequeueNotificationParser>(Lifestyle.Singleton);
        }
    }
}
