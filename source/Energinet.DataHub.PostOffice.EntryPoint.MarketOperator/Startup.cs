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
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Monitor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator
{
    internal sealed class Startup : StartupBase
    {
        protected override void Configure(IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetService<IConfiguration>() ?? throw new InvalidOperationException("IConfiguration not found");

            var serviceBusConnectionString = config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"] ?? throw new InvalidOperationException("Health check connection string not found");

            var timeSeriesQueue = config.GetValue("TIMESERIES_QUEUE_NAME", "timeseries");
            var timeSeriesReplyQueue = config.GetValue("TIMESERIES_REPLY_QUEUE_NAME", "timeseries-reply");
            var chargesQueue = config.GetValue("CHARGES_QUEUE_NAME", "charges");
            var chargesReplyQueue = config.GetValue("CHARGES_REPLY_QUEUE_NAME", "charges-reply");
            var marketRolesQueue = config.GetValue("MARKETROLES_QUEUE_NAME", "marketroles");
            var marketRolesReplyQueue = config.GetValue("MARKETROLES_REPLY_QUEUE_NAME", "marketroles-reply");
            var meteringPointsQueue = config.GetValue("METERINGPOINTS_QUEUE_NAME", "meteringpoints");
            var meteringPointsReplyQueue = config.GetValue("METERINGPOINTS_REPLY_QUEUE_NAME", "meteringpoints-reply");
            var aggregationsQueue = config.GetValue("AGGREGATIONS_QUEUE_NAME", "aggregations");
            var aggregationsReplyQueue = config.GetValue("AGGREGATIONS_REPLY_QUEUE_NAME", "aggregations-reply");

            var timeSeriesDequeueQueue = config.GetValue("TIMESERIES_DEQUEUE_QUEUE_NAME", "timeseries-dequeue");
            var chargesDequeueQueue = config.GetValue("CHARGES_DEQUEUE_QUEUE_NAME", "charges-dequeue");
            var marketRolesDequeueQueue = config.GetValue("MARKETROLES_DEQUEUE_QUEUE_NAME", "marketroles-dequeue");
            var meteringPointsDequeueQueue = config.GetValue("METERINGPOINTS_DEQUEUE_QUEUE_NAME", "meteringpoints-dequeue");
            var aggregationsDequeueQueue = config.GetValue("AGGREGATIONS_DEQUEUE_QUEUE_NAME", "aggregations-dequeue");

            // Health check
            services
                .AddHealthChecks()
                .AddLiveCheck()
                .AddCosmosDb(config["MESSAGES_DB_CONNECTION_STRING"])
                .AddAzureBlobStorage(config["BlobStorageConnectionString"])
                .AddSqlServer(config["SQL_ACTOR_DB_CONNECTION_STRING"])

                .AddAzureServiceBusQueue(serviceBusConnectionString, timeSeriesQueue, name: timeSeriesQueue)
                .AddAzureServiceBusQueue(serviceBusConnectionString, timeSeriesReplyQueue, name: timeSeriesReplyQueue)
                .AddAzureServiceBusQueue(serviceBusConnectionString, timeSeriesDequeueQueue, name: timeSeriesDequeueQueue)

                .AddAzureServiceBusQueue(serviceBusConnectionString, chargesQueue, name: chargesQueue)
                .AddAzureServiceBusQueue(serviceBusConnectionString, chargesReplyQueue, name: chargesReplyQueue)
                .AddAzureServiceBusQueue(serviceBusConnectionString, chargesDequeueQueue, name: chargesDequeueQueue)

                .AddAzureServiceBusQueue(serviceBusConnectionString, marketRolesQueue, name: marketRolesQueue)
                .AddAzureServiceBusQueue(serviceBusConnectionString, marketRolesReplyQueue, name: marketRolesReplyQueue)
                .AddAzureServiceBusQueue(serviceBusConnectionString, marketRolesDequeueQueue, name: marketRolesDequeueQueue)

                .AddAzureServiceBusQueue(serviceBusConnectionString, meteringPointsQueue, name: meteringPointsQueue)
                .AddAzureServiceBusQueue(serviceBusConnectionString, meteringPointsReplyQueue, name: meteringPointsReplyQueue)
                .AddAzureServiceBusQueue(serviceBusConnectionString, meteringPointsDequeueQueue, name: meteringPointsDequeueQueue)

                .AddAzureServiceBusQueue(serviceBusConnectionString, aggregationsQueue, name: aggregationsQueue)
                .AddAzureServiceBusQueue(serviceBusConnectionString, aggregationsReplyQueue, name: aggregationsReplyQueue)
                .AddAzureServiceBusQueue(serviceBusConnectionString, aggregationsDequeueQueue, name: aggregationsDequeueQueue);
        }

        protected override void Configure(Container container)
        {
            container.AddHttpAuthentication();
            container.Register<PeekFunction>(Lifestyle.Scoped);
            container.Register<PeekTimeSeriesFunction>(Lifestyle.Scoped);
            container.Register<PeekMasterDataFunction>(Lifestyle.Scoped);
            container.Register<PeekAggregationsFunction>(Lifestyle.Scoped);
            container.Register<DequeueFunction>(Lifestyle.Scoped);
            container.Register(() => new ExternalBundleIdProvider(), Lifestyle.Singleton);

            // health check
            container.Register<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>(Lifestyle.Scoped);
            container.Register<HealthCheckEndpoint>(Lifestyle.Scoped);
        }
    }
}
