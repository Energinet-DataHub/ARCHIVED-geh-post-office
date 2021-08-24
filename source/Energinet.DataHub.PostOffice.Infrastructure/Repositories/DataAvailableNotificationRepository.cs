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
using Energinet.DataHub.PostOffice.Domain;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public class DataAvailableNotificationRepository : IDataAvailableNotificationRepository
    {
        private const string ContainerName = "dataavailable";

        private readonly CosmosClient _cosmosClient;
        private readonly CosmosDatabaseConfig _cosmosConfig;
        private readonly Container _container;

        public DataAvailableNotificationRepository(
            CosmosClient cosmosClient,
            CosmosDatabaseConfig cosmosConfig)
        {
            _cosmosClient = cosmosClient;
            _cosmosConfig = cosmosConfig;
            _container = GetContainer(ContainerName);
        }

        public async Task CreateAsync(DataAvailableNotification dataAvailableNotification)
        {
            if (dataAvailableNotification is null)
                throw new ArgumentNullException(nameof(dataAvailableNotification));

            var cosmosDocument = new CosmosDataAvailable
            {
                uuid = dataAvailableNotification.Id.Value,
                recipient = dataAvailableNotification.Recipient.Value,
                messageType = dataAvailableNotification.MessageType.Type,
                origin = dataAvailableNotification.Origin.ToString(),
                relativeWeight = dataAvailableNotification.Weight.Value,
                priority = 1M,
            };

            var response = await _container.CreateItemAsync(cosmosDocument).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Created)
                throw new InvalidOperationException("Could not create document in cosmos");
        }

        public Task<IEnumerable<DataAvailableNotification>> PeekAsync(Recipient recipient, MessageType messageType)
        {
            return Task.FromResult(Enumerable.Empty<DataAvailableNotification>());
        }

        public async Task<DataAvailableNotification?> PeekAsync(Recipient recipient)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            const string queryString = "SELECT * FROM c WHERE c.recipient = @recipient ORDER BY c._ts ASC OFFSET 0 LIMIT 1";
            var parameters = new List<KeyValuePair<string, string>> { new("recipient", recipient.Value) };

            var documents = await GetDocumentsAsync(queryString, parameters).ConfigureAwait(false);
            var document = documents.FirstOrDefault();

            return document;
        }

        public async Task DequeueAsync(IEnumerable<Uuid> ids)
        {
            foreach (var uuid in ids)
            {
                var documentToUpdateResponse = await _container.ReadItemAsync<CosmosDataAvailable>(uuid.Value, new PartitionKey("recipient"));
                var documentToUpdate = documentToUpdateResponse.Resource;
                documentToUpdate.acknowledge = true;
                await _container.ReplaceItemAsync(documentToUpdate, uuid.Value, new PartitionKey(documentToUpdate.recipient));
            }
        }

        private async Task<IEnumerable<DataAvailableNotification>> GetDocumentsAsync(string query, List<KeyValuePair<string, string>> parameters)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            var documentQuery = new QueryDefinition(query);
            parameters.ForEach(item => documentQuery.WithParameter($"@{item.Key}", item.Value));

            var documentsResult = new List<DataAvailableNotification>();

            using (FeedIterator<CosmosDataAvailable> feedIterator = _container.GetItemQueryIterator<CosmosDataAvailable>(documentQuery))
            {
                while (feedIterator.HasMoreResults)
                {
                    var documentsFromCosmos = await feedIterator.ReadNextAsync().ConfigureAwait(false);
                    var documents = documentsFromCosmos
                        .Select(document => new DataAvailableNotification(
                            new Uuid(document.uuid),
                            new Recipient(document.recipient),
                            new MessageType(document.relativeWeight, document.messageType),
                            Enum.Parse<Origin>(document.origin, true),
                            new Weight(document.relativeWeight)));

                    documentsResult.AddRange(documents);
                }
            }

            return documentsResult;
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
