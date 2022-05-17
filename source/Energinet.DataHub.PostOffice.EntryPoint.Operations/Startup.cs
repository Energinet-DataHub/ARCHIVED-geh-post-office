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
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Common.Configuration;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.EntryPoint.Operations.Functions;
using Energinet.DataHub.PostOffice.EntryPoint.Operations.Monitor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.EntryPoint.Operations
{
    internal sealed class Startup : StartupBase
    {
        public Startup(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void Configure(IServiceCollection services)
        {
            // This is called to ensure the property is filled out. Actual value is read by Azure SDK.
            Configuration.GetSetting(Settings.MarketParticipantConnectionString);

            var cosmosDbConnectionString = Configuration.GetSetting(Settings.MessagesDbConnectionString);
            var serviceBusConnectionString = Configuration.GetSetting(Settings.ServiceBusHealthCheckConnectionString);
            var marketParticipantTopicName = Configuration.GetSetting(Settings.MarketParticipantTopicName);
            var marketParticipantSubscriptionName = Configuration.GetSetting(Settings.MarketParticipantSubscriptionName);

            // Health check
            services
                .AddHealthChecks()
                .AddLiveCheck()
                .AddCosmosDb(cosmosDbConnectionString)
                .AddAzureServiceBusSubscription(serviceBusConnectionString, marketParticipantTopicName, marketParticipantSubscriptionName);
        }

        protected override void Configure(Container container)
        {
            container.Register<ISharedIntegrationEventParser, SharedIntegrationEventParser>(Lifestyle.Singleton);

            container.Register<MarketParticipantIngestionFunction>(Lifestyle.Scoped);

            // health check
            container.Register<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>(Lifestyle.Scoped);
            container.Register<HealthCheckEndpoint>(Lifestyle.Scoped);
        }
    }
}
