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

using Energinet.DataHub.PostOffice.Common.Configuration;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.Infrastructure;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers.CosmosClients;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Container = SimpleInjector.Container;

namespace Energinet.DataHub.PostOffice.Common
{
    internal static class CosmosDbRegistration
    {
        public static void AddCosmosClientBuilder(this Container container)
        {
            container.RegisterSingleton<ICosmosBulkClient>(() => GetCosmosClient(container, true));
            container.RegisterSingleton<ICosmosClient>(() => GetCosmosClient(container, false));
        }

        public static void AddDatabaseCosmosConfig(this Container container)
        {
            container.RegisterSingleton(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var messageHubDatabaseId = configuration.GetSetting(Settings.MessagesDbId);
                return new CosmosDatabaseConfig(messageHubDatabaseId);
            });
        }

        private static CosmosClientProvider GetCosmosClient(Container container, bool bulkConfiguration)
        {
            var configuration = container.GetService<IConfiguration>();
            var connectionString = configuration.GetSetting(Settings.MessagesDbConnectionString);

            var cosmosSerializationOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };

            var cosmosClient = new CosmosClientBuilder(connectionString)
                .WithBulkExecution(bulkConfiguration)
                .WithSerializerOptions(cosmosSerializationOptions)
                .Build();

            return new CosmosClientProvider(cosmosClient);
        }
    }
}
