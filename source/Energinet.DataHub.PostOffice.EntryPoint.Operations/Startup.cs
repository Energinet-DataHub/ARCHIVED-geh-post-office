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
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.EntryPoint.Operations.Functions;
using Energinet.DataHub.PostOffice.EntryPoint.Operations.HealthCheck;
using Energinet.DataHub.PostOffice.EntryPoint.Operations.Monitor;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.EntryPoint.Operations
{
    internal sealed class Startup : StartupBase
    {
        protected override void Configure(IServiceCollection services)
        {
            // Health check
            services
                .AddHealthChecks()
                .AddLiveCheck();
        }

        protected override void Configure(Container container)
        {
            // market participant
            container.AddMarketParticipantConfig();
            container.Register<ICosmosDatabaseVerifier, CosmosDatabaseVerifier>(Lifestyle.Scoped);
            container.Register<ISqlDatabaseVerifier, SqlDatabaseVerifier>(Lifestyle.Scoped);
            container.Register<IServiceBusQueueVerifier, ServiceBusQueueVerifier>(Lifestyle.Scoped);
            container.Register<IHealth, Health>(Lifestyle.Scoped);

            // health check
            container.Register<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>(Lifestyle.Scoped);
            container.Register<HealthCheckEndpoint>(Lifestyle.Scoped);
        }
    }
}
