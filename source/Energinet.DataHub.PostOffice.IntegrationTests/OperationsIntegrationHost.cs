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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Energinet.DataHub.PostOffice.IntegrationTests
{
    public sealed class OperationsIntegrationHost : IAsyncDisposable
    {
        private readonly Startup _startup;

        private OperationsIntegrationHost()
        {
            _startup = new Startup();
        }

        public static Task<OperationsIntegrationHost> InitializeAsync()
        {
            var host = new OperationsIntegrationHost();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(BuildConfig());
            host._startup.ConfigureServices(serviceCollection);
            serviceCollection.BuildServiceProvider().UseSimpleInjector(host._startup.Container, o => o.Container.Options.EnableAutoVerification = false);
            host._startup.Container.Options.AllowOverridingRegistrations = true;

            return Task.FromResult(host);
        }

        public Scope BeginScope()
        {
            return AsyncScopedLifestyle.BeginScope(_startup.Container);
        }

        public async ValueTask DisposeAsync()
        {
            await _startup.DisposeAsync().ConfigureAwait(false);
        }

        private static IConfigurationRoot BuildConfig()
        {
            Environment.SetEnvironmentVariable("SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING", "SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING");

            return new ConfigurationBuilder().AddEnvironmentVariables().Build();
        }
    }
}
