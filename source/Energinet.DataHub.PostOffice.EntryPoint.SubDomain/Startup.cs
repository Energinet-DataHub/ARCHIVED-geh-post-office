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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Common.Configuration;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions;
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Monitor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain
{
    internal sealed class Startup : StartupBase
    {
        public Startup(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void Configure(IServiceCollection services)
        {
            var cosmosDbConnectionString = Configuration.GetSetting(Settings.MessagesDbConnectionString);
            var serviceBusConnectionString = Configuration.GetSetting(Settings.ServiceBusHealthCheckConnectionString);
            var dataAvailableQueueName = Configuration.GetSetting(Settings.DataAvailableQueueName);

            // Health check
            services
                .AddHealthChecks()
                .AddLiveCheck()
                .AddCosmosDb(cosmosDbConnectionString)
                .AddAzureServiceBusQueue(serviceBusConnectionString, dataAvailableQueueName);
        }

        protected override void Configure(Container container)
        {
            container.RegisterSingleton<IDataAvailableMessageReceiver>(() =>
            {
                var dataAvailableConnectionString = Configuration.GetSetting(Settings.DataAvailableConnectionString);
                var dataAvailableQueueName = Configuration.GetSetting(Settings.DataAvailableQueueName);

                var batchSize = Configuration.GetSetting(Settings.DataAvailableBatchSize);
                var timeoutInMs = Configuration.GetSetting(Settings.DataAvailableTimeoutMs);

                var serviceBusClient = new ServiceBusClient(dataAvailableConnectionString);

                var receiver = serviceBusClient.CreateReceiver(
                    dataAvailableQueueName,
                    new ServiceBusReceiverOptions { PrefetchCount = batchSize });

                return new DataAvailableMessageReceiver(receiver, batchSize, TimeSpan.FromMilliseconds(timeoutInMs));
            });

            container.Register<DataAvailableTimerTrigger>(Lifestyle.Scoped);

            // health check
            container.Register<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>(Lifestyle.Scoped);
            container.Register<HealthCheckEndpoint>(Lifestyle.Scoped);
        }
    }
}
