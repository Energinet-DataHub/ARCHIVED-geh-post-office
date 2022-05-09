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
using Energinet.DataHub.PostOffice.EntryPoint.Operations.Monitor;
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

            // Health check
            services.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
            services
                .AddHealthChecks()
                .AddLiveCheck()
                .AddCosmosDb(config["MESSAGES_DB_CONNECTION_STRING"])
                .AddAzureBlobStorage(config["BlobStorageConnectionString"])
                .AddSqlServer(config["SQL_ACTOR_DB_CONNECTION_STRING"])

                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["TIMESERIES_QUEUE_NAME"], name: "TIMESERIES_QUEUE_NAME")
                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["TIMESERIES_REPLY_QUEUE_NAME"], name: "TIMESERIES_REPLY_QUEUE_NAME")
                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["TIMESERIES_DEQUEUE_QUEUE_NAME"], name: "TIMESERIES_DEQUEUE_QUEUE_NAME")

                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["CHARGES_QUEUE_NAME"], name: "CHARGES_QUEUE_NAME")
                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["CHARGES_REPLY_QUEUE_NAME"], name: "CHARGES_REPLY_QUEUE_NAME")
                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["CHARGES_DEQUEUE_QUEUE_NAME"], name: "CHARGES_DEQUEUE_QUEUE_NAME")

                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["MARKETROLES_QUEUE_NAME"], name: "MARKETROLES_QUEUE_NAME")
                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["MARKETROLES_REPLY_QUEUE_NAME"], name: "MARKETROLES_REPLY_QUEUE_NAME")
                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["MARKETROLES_DEQUEUE_QUEUE_NAME"], name: "MARKETROLES_DEQUEUE_QUEUE_NAME")

                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["METERINGPOINTS_QUEUE_NAME"], name: "METERINGPOINTS_QUEUE_NAME")
                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["METERINGPOINTS_REPLY_QUEUE_NAME"], name: "METERINGPOINTS_REPLY_QUEUE_NAME")
                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["METERINGPOINTS_DEQUEUE_QUEUE_NAME"], name: "METERINGPOINTS_DEQUEUE_QUEUE_NAME")

                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["AGGREGATIONS_QUEUE_NAME"], name: "AGGREGATIONS_QUEUE_NAME")
                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["AGGREGATIONS_REPLY_QUEUE_NAME"], name: "AGGREGATIONS_REPLY_QUEUE_NAME")
                .AddAzureServiceBusQueue(config["SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING"], config["AGGREGATIONS_DEQUEUE_QUEUE_NAME"], name: "AGGREGATIONS_DEQUEUE_QUEUE_NAME");
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

            // Health check
            container.Register<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>(Lifestyle.Scoped);
            container.Register<HealthCheckEndpoint>(Lifestyle.Scoped);
        }
    }
}
