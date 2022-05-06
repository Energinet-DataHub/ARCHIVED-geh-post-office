﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions;
using Energinet.DataHub.PostOffice.EntryPoint.Operations.Monitor;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator
{
    internal sealed class Startup : StartupBase
    {
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
