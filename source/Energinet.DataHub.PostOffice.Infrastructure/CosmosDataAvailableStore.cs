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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using Energinet.DataHub.PostOffice.Domain;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure
{
    public class CosmosDataAvailableStore : IDocumentStore<DataAvailable>
    {
        private const string ContainerName = "dataavailable";

        private readonly CosmosClient _cosmosClient;
        private readonly CosmosDatabaseConfig _cosmosConfig;

        public CosmosDataAvailableStore(
            CosmosClient cosmosClient,
            CosmosDatabaseConfig cosmosConfig)
        {
            _cosmosClient = cosmosClient;
            _cosmosConfig = cosmosConfig;
        }

        public async Task<IEnumerable<DataAvailable>> GetDocumentsAsync(string query, List<KeyValuePair<string, string>> parameters)
        {
            if (query is null) throw new ArgumentNullException(nameof(query));
            if (parameters is null) throw new ArgumentNullException(nameof(parameters));

            var documentQuery = new QueryDefinition(query);
            parameters.ForEach(item => documentQuery.WithParameter($"@{item.Key}", item.Value));

            var container = GetContainer(ContainerName);

            using (FeedIterator<CosmosDataAvailable> feedIterator = container.GetItemQueryIterator<CosmosDataAvailable>(documentQuery))
            {
                var documentsFromCosmos = await feedIterator.ReadNextAsync().ConfigureAwait(false);
                var documents = documentsFromCosmos
                    .Select(document => new DataAvailable(
                        document.uuid,
                        document.recipient,
                        document.messageType,
                        document.origin,
                        document.supportsBundling,
                        document.relativeWeight,
                        document.priority));

                return documents.ToList();
            }
        }

        public async Task<DataAvailable?> GetOldestDocumentAsync(GetMessageQuery documentQuery)
        {
            if (documentQuery is null) throw new ArgumentNullException(nameof(documentQuery));

            const string QueryString = @"
                SELECT TOP 1 *
                FROM Documents d
                WHERE d.recipient = @recipient
                ORDER BY d.EffectuationDate";

            var container = GetContainer(ContainerName);

            // Querying with an equality filter on the partition key will create a partitioned documentQuery, per:
            // https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-documentQuery-container#in-partition-documentQuery
            // TODO: Change when actual names are available, ie. recipient_MriD?
            var queryDefinition = new QueryDefinition(QueryString)
                .WithParameter("@recipient", documentQuery.Recipient);

            using (FeedIterator<CosmosDataAvailable> feedIterator =
                container.GetItemQueryIterator<CosmosDataAvailable>(queryDefinition))
            {
                var documents = new List<DataAvailable>();
                var documentsFromCosmos = await feedIterator.ReadNextAsync().ConfigureAwait(false);
                foreach (var document in documentsFromCosmos)
                {
                    documents.Add(new DataAvailable(
                        document.uuid,
                        document.recipient,
                        document.messageType,
                        document.origin,
                        document.supportsBundling,
                        document.relativeWeight,
                        document.priority));
                }

                return documents.FirstOrDefault();
            }
        }

        public Task<IList<DataAvailable>> GetDocumentBundleAsync(GetMessageQuery documentQuery)
        {
            throw new NotImplementedException();
        }

        public Task SaveDocumentAsync(DataAvailable document, string containerName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteDocumentsAsync(DequeueCommand dequeueCommand)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveDocumentAsync(DataAvailable document)
        {
            if (document is null) throw new ArgumentNullException(nameof(document));

            var container = GetContainer(ContainerName);

            var cosmosDocument = new CosmosDataAvailable
            {
                uuid = document.Uuid,
                recipient = document.Recipient,
                messageType = document.MessageType,
                origin = document.Origin,
                supportsBundling = document.SupportsBundling,
                relativeWeight = document.RelativeWeight,
                priority = document.Priority,
            };

            var response = await container.CreateItemAsync(cosmosDocument).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException("Could not create document in cosmos");
            }

            return true;
        }

        private Container GetContainer(string containerName)
        {
            var container = _cosmosClient.GetContainer(
                _cosmosConfig.DatabaseId,
                containerName);
            return container;
        }
    }
}
