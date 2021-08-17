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
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Inbound;
using Energinet.DataHub.PostOffice.Infrastructure;
using Energinet.DataHub.PostOffice.IntegrationTests.DataAvailable;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Energinet.DataHub.PostOffice.IntegrationTests
{
    public sealed class InboundIntegrationTestHost : IAsyncDisposable
    {
        private readonly Scope _scope;
        private readonly Startup _startup;

        public InboundIntegrationTestHost()
        {
            InitSettings();
            _startup = new Startup();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(BuildConfig());
            _startup.ConfigureServices(serviceCollection);

            // todo mjm : temp mocking of Cosmos
            _startup.Container.Register(BuildMoqCosmosClient, Lifestyle.Scoped);
            serviceCollection.BuildServiceProvider().UseSimpleInjector(_startup.Container, o => o.Container.Options.EnableAutoVerification = false);

            _scope = AsyncScopedLifestyle.BeginScope(_startup.Container);
        }

        public async ValueTask DisposeAsync()
        {
            await _scope.DisposeAsync().ConfigureAwait(false);
            await _startup.DisposeAsync().ConfigureAwait(false);
        }

        public TService GetService<TService>()
            where TService : class
        {
            return _startup.Container.GetInstance<TService>();
        }

        private static IConfigurationRoot BuildConfig()
        {
            return new ConfigurationBuilder().AddEnvironmentVariables().Build();
        }

        // todo mjm : temp initSttings
        private static void InitSettings()
        {
            Environment.SetEnvironmentVariable("MESSAGES_DB_NAME", "fake_value");
        }

        // todo mjm : github action creating test-config
        /*private static void InitSettings()
        {
            // Hack to include settings.json as xUnit does not include appSettings automatically as environment variables
            // https://github.com/Azure/azure-functions-host/issues/6953
            var settings = File.ReadAllText("local.settings.json");
            var json = JObject.Parse(settings);
            var values = json.Value<JObject>("Values");
            foreach (var (key, value) in values)
                Environment.SetEnvironmentVariable(key, value.ToString());
        }*/

        private static CosmosClient BuildMoqCosmosClient()
        {
            var mockItemResponse = new Mock<ItemResponse<CosmosDataAvailable>>();
            mockItemResponse.Setup(x => x.StatusCode)
                .Returns(HttpStatusCode.Created);

            var mockContainer = new Mock<Microsoft.Azure.Cosmos.Container>();
            mockContainer
                .Setup(e => e.CreateItemAsync(
                    It.IsAny<CosmosDataAvailable>(),
                    null,
                    null,
                    default))
                .ReturnsAsync(mockItemResponse.Object);

            return new CosmosClientWrapper(
                "AccountEndpoint=https://localhost:8081/;AccountKey=1234",
                mockContainer);
        }
    }
}
