using System;
using Energinet.DataHub.PostOffice.Infrastructure;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Container = SimpleInjector.Container;

namespace Energinet.DataHub.PostOffice.Common
{
    internal static class CosmosDbRegistration
    {
        public static void AddCosmosClientBuilder(this Container container, bool useBulkExecution)
        {
            container.RegisterSingleton(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var connectionString = configuration.GetConnectionStringOrSetting("MESSAGES_DB_CONNECTION_STRING");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid CosmosDBConnection in the appSettings.json file or your Azure Functions Settings.");
                }

                return new CosmosClientBuilder(connectionString)
                    .WithBulkExecution(useBulkExecution)
                    .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                    .Build();
            });
        }

        public static void AddDatabaseCosmosConfig(this Container container)
        {
            container.RegisterSingleton(
                () =>
                {
                    var configuration = container.GetService<IConfiguration>();
                    var databaseId = configuration.GetValue<string>("MESSAGES_DB_NAME");

                    return new CosmosDatabaseConfig(databaseId);
                });
        }
    }
}
