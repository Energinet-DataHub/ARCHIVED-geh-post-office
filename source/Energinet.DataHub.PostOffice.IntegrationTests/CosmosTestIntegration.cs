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
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;

namespace Energinet.DataHub.PostOffice.IntegrationTests
{
    internal static class CosmosTestIntegration
    {
        private const string AzureCosmosDatabaseName = "post-office";
        private const string AzureCosmosLogDatabaseName = "Log";
        private const string AzureCosmosEmulatorConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        public static async Task InitializeAsync()
        {
            Environment.SetEnvironmentVariable("MESSAGES_DB_NAME", AzureCosmosDatabaseName);
            Environment.SetEnvironmentVariable("MESSAGES_DB_CONNECTION_STRING", AzureCosmosEmulatorConnectionString);
            Environment.SetEnvironmentVariable("LOG_DB_NAME", AzureCosmosLogDatabaseName);

            using var cosmosClient = new CosmosClient(AzureCosmosEmulatorConnectionString);

            var databaseResponse = await cosmosClient
                .CreateDatabaseIfNotExistsAsync(AzureCosmosDatabaseName)
                .ConfigureAwait(true);

            var logDatabaseResponse = await cosmosClient
                .CreateDatabaseIfNotExistsAsync(AzureCosmosLogDatabaseName)
                .ConfigureAwait(true);

            var testDatabase = databaseResponse.Database;
            var logDatabase = logDatabaseResponse.Database;

            await testDatabase
                .CreateContainerIfNotExistsAsync("dataavailable", "/recipient")
                .ConfigureAwait(true);

            var bundlesResponse = await testDatabase
                .CreateContainerIfNotExistsAsync("bundles", "/recipient")
                .ConfigureAwait(true);

            await logDatabase
                .CreateContainerIfNotExistsAsync("Endpoint-All", "/recipient")
                .ConfigureAwait(true);

            var singleBundleViolationTrigger = new TriggerProperties
            {
                Id = "EnsureSingleUnacknowledgedBundle",
                TriggerOperation = TriggerOperation.Create,
                TriggerType = TriggerType.Post,
                Body = @"
function trigger() {

    var context = getContext();
    var container = context.getCollection();
    var response = context.getResponse();
    var createdItem = response.getBody();

    // Query for checking if there are other unacknowledged bundles for market operator.
    var filterQuery = `
    SELECT * FROM bundles b
    WHERE b.recipient = '${createdItem.recipient}' AND
          b.dequeued = false AND (
          b.origin = '${createdItem.origin}' OR
         (b.origin = 'TimeSeries' AND '${createdItem.origin}' = 'Aggregations') OR
         (b.origin = 'Aggregations' AND '${createdItem.origin}' = 'TimeSeries'))`;

    var accept = container.queryDocuments(container.getSelfLink(), filterQuery, function(err, items, options)
    {
        if (err) throw err;
        if (items.length !== 0) throw 'SingleBundleViolation';
    });

    if (!accept) throw 'queryDocuments in trigger failed.';
}"
            };

            var bundles = bundlesResponse.Container;
            var scripts = bundles.Scripts;

            try
            {
                await scripts.DeleteTriggerAsync(singleBundleViolationTrigger.Id).ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Trigger not there, ignore.
            }

            await scripts.CreateTriggerAsync(singleBundleViolationTrigger).ConfigureAwait(false);
        }
    }
}
