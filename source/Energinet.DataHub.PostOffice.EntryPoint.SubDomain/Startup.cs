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
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions;
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Monitor;
using Energinet.DataHub.PostOffice.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain
{
    internal sealed class Startup : StartupBase
    {
        protected override void Configure(IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetService<IConfiguration>() ?? throw new InvalidOperationException("IConfiguration not found");
            var serviceBusConnectionString = config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"] ?? throw new InvalidOperationException("Health check connection string not found");

            // Health check
            services
                .AddHealthChecks()
                .AddLiveCheck()
                .AddCosmosDb(config["MESSAGES_DB_CONNECTION_STRING"])
                .AddAzureServiceBusQueue(serviceBusConnectionString, config["DATAAVAILABLE_QUEUE_NAME"]);
        }

        protected override void Configure(Container container)
        {
            container.RegisterSingleton<IDataAvailableMessageReceiver>(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var batchSize = configuration.GetValue("DATAAVAILABLE_BATCH_SIZE", 10000);
                var timeoutInMs = configuration.GetValue("DATAAVAILABLE_TIMEOUT_IN_MS", 1000);

                var serviceBusConfig = container.GetInstance<DataAvailableServiceBusConfig>();
                var serviceBusClient = new ServiceBusClient(serviceBusConfig.DataAvailableQueueConnectionString);
                var receiver = serviceBusClient.CreateReceiver(
                    serviceBusConfig.DataAvailableQueueName,
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
