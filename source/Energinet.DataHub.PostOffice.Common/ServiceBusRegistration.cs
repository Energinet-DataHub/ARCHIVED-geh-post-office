// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.PostOffice.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Container = SimpleInjector.Container;

namespace Energinet.DataHub.PostOffice.Common
{
    public static class ServiceBusRegistration
    {
        public static void AddMarketParticipantServiceBus(this Container container)
        {
            ArgumentNullException.ThrowIfNull(container, nameof(container));

            container.RegisterSingleton(() =>
            {
                var configuration = container.GetService<IConfiguration>();

                var marketParticipantConnectionString = configuration.GetValue<string>(MarketParticipantServiceBusConfig.MarketParticipantConnectionStringKey);

                var marketParticipantTopicName = configuration.GetValue<string>(MarketParticipantServiceBusConfig.MarketParticipantTopicNameKey);

                var marketParticipantSubscriptionName = configuration.GetValue<string>(MarketParticipantServiceBusConfig.MarketParticipantSubscriptionNameKey);

                return new MarketParticipantServiceBusConfig(
                    marketParticipantConnectionString,
                    marketParticipantTopicName,
                    marketParticipantSubscriptionName);
            });
        }

        internal static void AddDataAvailableServiceBus(this Container container)
        {
            container.RegisterSingleton(() =>
            {
                var configuration = container.GetService<IConfiguration>();

                var dataAvailableQueueName = configuration.GetValue<string>(DataAvailableServiceBusConfig.DataAvailableQueueNameKey);
                var dataAvailableQueueConnectionString = configuration.GetValue<string>(DataAvailableServiceBusConfig.DataAvailableQueueConnectionStringKey);

                return new DataAvailableServiceBusConfig(dataAvailableQueueName, dataAvailableQueueConnectionString);
            });

            container.RegisterSingleton<IServiceBusClientFactory>(() =>
            {
                var configuration = container.GetInstance<DataAvailableServiceBusConfig>();
                return new ServiceBusClientFactory(configuration.DataAvailableQueueConnectionString);
            });

            container.RegisterSingleton<IMessageBusFactory>(() =>
            {
                var serviceBusClientFactory = container.GetInstance<IServiceBusClientFactory>();
                return new AzureServiceBusFactory(serviceBusClientFactory);
            });
        }
    }
}
