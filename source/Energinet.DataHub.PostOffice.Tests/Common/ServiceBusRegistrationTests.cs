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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using SimpleInjector;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common
{
    [UnitTest]
    public class ServiceBusRegistrationTests
    {
        private const string FakeConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test";

        [Fact]
        public async Task AddDataAvailableServiceBus_AllGood_IsRegistered()
        {
            // arrange
            var configuration = CreateConfiguration(
                (DataAvailableServiceBusConfig.DataAvailableQueueConnectionStringKey, FakeConnectionString),
                (DataAvailableServiceBusConfig.DataAvailableQueueNameKey, "fake_value"));

            await using var container = new Container();
            container.Register(() => configuration, Lifestyle.Singleton);

            // act
            container.AddDataAvailableServiceBus();
            var actual = container.GetInstance<IServiceBusClientFactory>();
            var config = container.GetInstance<DataAvailableServiceBusConfig>();

            // assert
            Assert.NotNull(actual);
            Assert.NotNull(config);
        }

        [Fact]
        public async Task AddMarketParticipantServiceBus_AllGood_IsRegistered()
        {
            // arrange
            var configuration = CreateConfiguration(
                (MarketParticipantServiceBusConfig.MarketParticipantConnectionStringKey, FakeConnectionString),
                (MarketParticipantServiceBusConfig.MarketParticipantTopicNameKey, "fake_value"),
                (MarketParticipantServiceBusConfig.MarketParticipantSubscriptionNameKey, "fake_value"));

            await using var container = new Container();
            container.Register(() => configuration, Lifestyle.Singleton);

            // act
            container.AddMarketParticipantServiceBus();
            var actual = container.GetInstance<MarketParticipantServiceBusConfig>();

            // assert
            Assert.NotNull(actual);
        }

        [Fact]
        public async Task AddDataAvailableServiceBus_NoConnectionString_Throws()
        {
            // arrange
            var configuration = CreateConfiguration(
                (DataAvailableServiceBusConfig.DataAvailableQueueConnectionStringKey, string.Empty),
                (DataAvailableServiceBusConfig.DataAvailableQueueNameKey, "fake_value"));

            await using var container = new Container();
            container.Register(() => configuration, Lifestyle.Singleton);

            // act
            container.AddDataAvailableServiceBus();

            // assert
            Assert.Throws<ActivationException>(() => container.GetInstance<IServiceBusClientFactory>());
        }

        [Fact]
        public async Task AddMarketParticipantServiceBus_NoConnectionString_Throws()
        {
            // arrange
            var configuration = CreateConfiguration(
                (MarketParticipantServiceBusConfig.MarketParticipantConnectionStringKey, string.Empty),
                (MarketParticipantServiceBusConfig.MarketParticipantTopicNameKey, "fake_value"),
                (MarketParticipantServiceBusConfig.MarketParticipantSubscriptionNameKey, "fake_value"));

            await using var container = new Container();
            container.Register(() => configuration, Lifestyle.Singleton);

            // act
            container.AddMarketParticipantServiceBus();

            // assert
            Assert.Throws<ActivationException>(() => container.GetInstance<MarketParticipantServiceBusConfig>());
        }

        private static IConfiguration CreateConfiguration(params (string Key, string Value)[] config)
        {
            var keyValuePairs = config
                .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value));

            var memoryConfig = new MemoryConfigurationSource { InitialData = keyValuePairs };
            var configuration = new ConfigurationBuilder();
            configuration.Add(memoryConfig);
            return configuration.Build();
        }
    }
}
