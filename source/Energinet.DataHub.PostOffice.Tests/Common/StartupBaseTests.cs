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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.Common.Configuration;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers.CosmosClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleInjector;
using Xunit;
using Xunit.Categories;
using Container = SimpleInjector.Container;

namespace Energinet.DataHub.PostOffice.Tests.Common
{
    [UnitTest]
    public sealed class StartupBaseTests
    {
        [Fact]
        public async Task Startup_ConfigureServices_ShouldVerify()
        {
            // Arrange
            var configuration = BuildConfig();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(configuration);
            await using var target = new TestOfStartupBase();

            // Act
            target.ConfigureServices(configuration, serviceCollection);
            await using var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.UseSimpleInjector(target.Container);

            // Assert
            target.Container.Verify();
        }

        [Fact]
        public async Task Startup_ConfigureServices_ShouldCallConfigureContainer()
        {
            // Arrange
            var configuration = BuildConfig();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(configuration);
            var configureContainerMock = new Mock<Action>();
            await using var target = new TestOfStartupBase()
            {
                ConfigureContainer = configureContainerMock.Object
            };

            // Act
            target.ConfigureServices(configuration, serviceCollection);

            // Assert
            configureContainerMock.Verify(x => x(), Times.Once);
        }

        private static IConfiguration BuildConfig()
        {
            KeyValuePair<string, string>[] keyValuePairs =
            {
                new(Settings.MessagesDbId.Key, "fake_value"),
                new(Settings.BlobStorageContainerName.Key, "fake_value"),
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(keyValuePairs)
                .Build();
        }

        private sealed class TestOfStartupBase : StartupBase
        {
            public Action? ConfigureContainer { get; init; }

            protected override void Configure(IConfiguration configuration, IServiceCollection services)
            {
            }

            protected override void Configure(IConfiguration configuration, Container container)
            {
                AddMockConfiguration(container);
                ConfigureContainer?.Invoke();
            }

            private static void AddMockConfiguration(Container container)
            {
                container.Options.AllowOverridingRegistrations = true;
                container.RegisterSingleton<ServiceBusClient>(() => new MockedServiceBusClient());
                container.RegisterSingleton<ICosmosBulkClient>(() => new CosmosClientProvider(new MockedCosmosClient()));
                container.RegisterSingleton<ICosmosClient>(() => new CosmosClientProvider(new MockedCosmosClient()));
                container.RegisterSingleton<IServiceBusClientFactory>(() => new MockedServiceBusClientFactory(new MockedServiceBusClient()));
                container.RegisterSingleton<IStorageServiceClientFactory>(() => new MockedStorageServiceClientFactory());
            }
        }
    }
}
