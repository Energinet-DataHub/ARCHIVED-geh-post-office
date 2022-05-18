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

using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Storage;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.Common.Configuration;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Monitor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator
{
    internal sealed class Startup : StartupBase
    {
        protected override void Configure(IConfiguration configuration, IServiceCollection services)
        {
            var cosmosDbConnectionString = configuration.GetSetting(Settings.MessagesDbConnectionString);
            var serviceBusConnectionString = configuration.GetSetting(Settings.ServiceBusHealthCheckConnectionString);
            var sqlActorDbConnectionString = configuration.GetSetting(Settings.SqlActorDbConnectionString);
            var blobStorageConnectionString = configuration.GetSetting(Settings.BlobStorageConnectionString);

            var timeSeriesQueue = configuration.GetSetting(Settings.TimeSeriesQueue);
            var timeSeriesReplyQueue = configuration.GetSetting(Settings.TimeSeriesReplyQueue);
            var chargesQueue = configuration.GetSetting(Settings.ChargesQueue);
            var chargesReplyQueue = configuration.GetSetting(Settings.ChargesReplyQueue);
            var marketRolesQueue = configuration.GetSetting(Settings.MarketRolesQueue);
            var marketRolesReplyQueue = configuration.GetSetting(Settings.MarketRolesReplyQueue);
            var meteringPointsQueue = configuration.GetSetting(Settings.MeteringPointsQueue);
            var meteringPointsReplyQueue = configuration.GetSetting(Settings.MeteringPointsReplyQueue);
            var aggregationsQueue = configuration.GetSetting(Settings.AggregationsQueue);
            var aggregationsReplyQueue = configuration.GetSetting(Settings.AggregationsReplyQueue);

            var timeSeriesDequeueQueue = configuration.GetSetting(Settings.TimeSeriesDequeueQueue);
            var chargesDequeueQueue = configuration.GetSetting(Settings.ChargesDequeueQueue);
            var marketRolesDequeueQueue = configuration.GetSetting(Settings.MarketRolesDequeueQueue);
            var meteringPointsDequeueQueue = configuration.GetSetting(Settings.MeteringPointsDequeueQueue);
            var aggregationsDequeueQueue = configuration.GetSetting(Settings.AggregationsDequeueQueue);

            // Health check
            services
                .AddHealthChecks()
                .AddLiveCheck()
                .AddCosmosDb(cosmosDbConnectionString)
                .AddAzureBlobStorage(blobStorageConnectionString)
                .AddSqlServer(sqlActorDbConnectionString)

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

        protected override void Configure(IConfiguration configuration, Container container)
        {
            container.AddHttpAuthentication();
            container.Register<PeekFunction>(Lifestyle.Scoped);
            container.Register<PeekTimeSeriesFunction>(Lifestyle.Scoped);
            container.Register<PeekMasterDataFunction>(Lifestyle.Scoped);
            container.Register<PeekAggregationsFunction>(Lifestyle.Scoped);
            container.Register<DequeueFunction>(Lifestyle.Scoped);
            container.Register(() => new ExternalBundleIdProvider(), Lifestyle.Singleton);

            AddRequestResponseLogging(container);

            // health check
            container.Register<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>(Lifestyle.Scoped);
            container.Register<HealthCheckEndpoint>(Lifestyle.Scoped);
        }

        private static void AddRequestResponseLogging(Container container)
        {
            container.Register<RequestResponseLoggingMiddleware>(Lifestyle.Scoped);
            container.RegisterSingleton<IRequestResponseLogging>(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var connectionString = configuration.GetSetting(Settings.RequestResponseLogConnectionString);
                var containerName = configuration.GetSetting(Settings.RequestResponseLogContainerName);

                var logger = container.GetService<ILogger<RequestResponseLoggingBlobStorage>>();
                return new RequestResponseLoggingBlobStorage(connectionString, containerName, logger!);
            });
        }
    }
}
