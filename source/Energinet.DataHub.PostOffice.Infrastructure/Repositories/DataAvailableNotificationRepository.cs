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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Correlation;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers.CosmosClients;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public class DataAvailableNotificationRepository : IDataAvailableNotificationRepository
    {
        private readonly IDataAvailableNotificationRepositoryContainer _repositoryContainer;
        private readonly ILogCallback _logCallback;

        public DataAvailableNotificationRepository(
            IDataAvailableNotificationRepositoryContainer repositoryContainer,
            ILogCallback logCallback)
        {
            _repositoryContainer = repositoryContainer;
            _logCallback = logCallback;
        }

        public async Task SaveAsync(IBundleableNotifications bundleableNotifications)
        {
            if (bundleableNotifications is null)
                throw new ArgumentNullException(nameof(bundleableNotifications));

            var notifications = bundleableNotifications.GetNotifications();

            var nextPartition = await FindNextAvailablePartitionAsync(bundleableNotifications.PartitionKey).ConfigureAwait(false);

            var nextPartitionSize = nextPartition is null ? 0 : await GetPartitionSizeAsync(nextPartition.QueueKey).ConfigureAwait(false);

            foreach (var notification in notifications)
            {
                var domainMessageType = new CosmosDomainMessageType(
                    notification.Recipient.Gln.Value,
                    notification.Origin.ToString(),
                    notification.ContentType.Value,
                    notification.SequenceNumber.Value);

                if (nextPartition is null)
                {
                    nextPartition = CreateNewSubPartition(bundleableNotifications.PartitionKey);
                    nextPartitionSize = 0;

                    await _repositoryContainer.Container.CreateItemAsync(nextPartition).ConfigureAwait(false);

                    await _repositoryContainer.Container.CreateItemAsync(domainMessageType).ConfigureAwait(false);
                }

                if (nextPartition.PartitionIndex == nextPartitionSize)
                {
                    await _repositoryContainer.Container.CreateItemAsync(domainMessageType).ConfigureAwait(false);
                }

                notification.PartitionKey = nextPartition.PartitionKey;
                await _repositoryContainer.Container.CreateItemAsync(notification).ConfigureAwait(false);
                nextPartitionSize++;

                if (nextPartitionSize.Equals(10000))
                {
                    nextPartitionSize = 0;
                }
            }
        }

        public static CosmosPartitionDescriptor CreateNewSubPartition(BundleableNotificationsKey key)
        {

        }

        public async Task<CosmosPartitionDescriptor?> FindNextAvailablePartitionAsync(BundleableNotificationsKey bundleableNotificationsKey)
        {
            var asLinq = _repositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosPartitionDescriptor>();

            var query =
                from dataAvailable in asLinq
                where
                    dataAvailable.PartitionKey == bundleableNotificationsKey.PartitionKey && dataAvailable.PartitionIndex < 10000
                orderby dataAvailable.InitialSequenceNumber descending
                select dataAvailable;

            return await ExecuteQueryAsync(query)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<int> GetPartitionSizeAsync(Guid queueKey)
        {
            var asLinq = _repositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailable in asLinq
                where
                    dataAvailable.PartitionKey == queueKey.ToString()
                select dataAvailable;

            return await query.CountAsync().ConfigureAwait(false);
        }

        public Task SaveAsync(DataAvailableNotification dataAvailableNotification)
        {
            if (dataAvailableNotification is null)
                throw new ArgumentNullException(nameof(dataAvailableNotification));

            var cosmosDocument = new CosmosDataAvailable
            {
                Id = dataAvailableNotification.NotificationId.ToString(),
                Recipient = dataAvailableNotification.Recipient.Gln.Value,
                ContentType = dataAvailableNotification.ContentType.Value,
                Origin = dataAvailableNotification.Origin.ToString(),
                SupportsBundling = dataAvailableNotification.SupportsBundling.Value,
                RelativeWeight = dataAvailableNotification.Weight.Value,
                Acknowledge = false,
                PartitionKey = dataAvailableNotification.Recipient.Gln.Value + dataAvailableNotification.Origin + dataAvailableNotification.ContentType.Value
            };

            return _repositoryContainer.Container.CreateItemAsync(cosmosDocument);
        }

        public async Task SaveAsync(IEnumerable<DataAvailableNotification> dataAvailableNotifications)
        {
            if (dataAvailableNotifications is null)
                throw new ArgumentNullException(nameof(dataAvailableNotifications));

            var concurrentTasks = new List<Task>();

            foreach (var dataAvailableNotification in dataAvailableNotifications)
            {
                var item = new CosmosDataAvailable
                {
                    Id = dataAvailableNotification.NotificationId.ToString(),
                    Recipient = dataAvailableNotification.Recipient.Gln.Value,
                    ContentType = dataAvailableNotification.ContentType.Value,
                    Origin = dataAvailableNotification.Origin.ToString(),
                    SupportsBundling = dataAvailableNotification.SupportsBundling.Value,
                    RelativeWeight = dataAvailableNotification.Weight.Value,
                    Acknowledge = false,
                    PartitionKey = dataAvailableNotification.Recipient.Gln.Value + dataAvailableNotification.Origin + dataAvailableNotification.ContentType.Value
                };

                concurrentTasks.Add(_repositoryContainer.Container.CreateItemAsync(item));
            }

            await Task.WhenAll(concurrentTasks).ConfigureAwait(false);
        }

        public async Task<DataAvailableNotification?> GetNextUnacknowledgedAsync(MarketOperator recipient, params DomainOrigin[] domains)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            var sw = Stopwatch.StartNew();

            var asLinq = _repositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosDataAvailable>();

            IQueryable<CosmosDataAvailable> domainFiltered = asLinq;

            if (domains is { Length: > 0 })
            {
                var selectedDomains = domains.Select(x => x.ToString());
                domainFiltered = asLinq.Where(x => selectedDomains.Contains(x.Origin));
            }

            var query =
                from dataAvailable in domainFiltered
                where
                    dataAvailable.Recipient == recipient.Gln.Value &&
                    !dataAvailable.Acknowledge
                orderby dataAvailable.Timestamp
                select dataAvailable;

            var firstOrDefaultAsync = await ExecuteQueryAsync(query)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            _logCallback.Log($"GetNextUnacknowledgedAsync (Single): {sw.ElapsedMilliseconds} ms.\n");

            return firstOrDefaultAsync;
        }

        public async Task<IEnumerable<DataAvailableNotification>> GetNextUnacknowledgedAsync(
            MarketOperator recipient,
            DomainOrigin domainOrigin,
            ContentType contentType,
            Weight maxWeight)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            if (contentType is null)
                throw new ArgumentNullException(nameof(contentType));

            var sw = Stopwatch.StartNew();

            var asLinq = _repositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailable in asLinq
                where
                    dataAvailable.Recipient == recipient.Gln.Value &&
                    dataAvailable.ContentType == contentType.Value &&
                    dataAvailable.Origin == domainOrigin.ToString() &&
                    !dataAvailable.Acknowledge
                orderby dataAvailable.Timestamp
                select dataAvailable;

            var currentWeight = new Weight(0);
            var allUnacknowledged = new List<DataAvailableNotification>();

            await foreach (var item in ExecuteBatchAsync(query).ConfigureAwait(false))
            {
                if (allUnacknowledged.Count == 0 || (currentWeight + item.Weight <= maxWeight && item.SupportsBundling.Value))
                {
                    currentWeight += item.Weight;
                    allUnacknowledged.Add(item);
                }
                else
                {
                    break;
                }
            }

            _logCallback.Log($"GetNextUnacknowledgedAsync (List): {sw.ElapsedMilliseconds} ms.\n");

            return allUnacknowledged;
        }

        public async Task AcknowledgeAsync(MarketOperator recipient, IEnumerable<Uuid> dataAvailableNotificationUuids)
        {
            if (recipient is null)
                throw new ArgumentNullException(nameof(recipient));

            if (dataAvailableNotificationUuids is null)
                throw new ArgumentNullException(nameof(dataAvailableNotificationUuids));

            var stringIds = dataAvailableNotificationUuids
                .Select(x => x.ToString());

            var container = _repositoryContainer.Container;
            var asLinq = container
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailable in asLinq
                where dataAvailable.Recipient == recipient.Gln.Value && stringIds.Contains(dataAvailable.Id)
                select dataAvailable;

            TransactionalBatch? batch = null;

            var batchSize = 0;

            await foreach (var document in query.AsCosmosIteratorAsync().ConfigureAwait(false))
            {
                var updatedDocument = document with { Acknowledge = true };

                batch ??= container.CreateTransactionalBatch(new PartitionKey(updatedDocument.PartitionKey));
                batch.ReplaceItem(updatedDocument.Id, updatedDocument);

                batchSize++;

                // Microsoft decided on an arbitrary batch limit of 100.
                if (batchSize == 100)
                {
                    using var innerResult = await batch.ExecuteAsync().ConfigureAwait(false);

                    // As written in docs, _this_ API does not throw exceptions and has to be checked.
                    if (!innerResult.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(innerResult.ErrorMessage);
                    }

                    batch = null;
                    batchSize = 0;
                }
            }

            if (batch != null)
            {
                using var outerResult = await batch.ExecuteAsync().ConfigureAwait(false);

                // As written in docs, _this_ API does not throw exceptions and has to be checked.
                if (!outerResult.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(outerResult.ErrorMessage);
                }
            }
        }

        public async Task WriteToArchiveAsync(IEnumerable<Uuid> dataAvailableNotifications, string partitionKey)
        {
            if (partitionKey is null)
                throw new ArgumentNullException(nameof(partitionKey));

            var documentPartitionKey = new PartitionKey(partitionKey);
            var documentsToRead = dataAvailableNotifications.Select(e => (e.ToString(), documentPartitionKey)).ToList();

            var sw1 = Stopwatch.StartNew();

            var documentsToArchive = await _repositoryContainer
                .Container
                .ReadManyItemsAsync<CosmosDataAvailable>(documentsToRead).ConfigureAwait(false);

            _logCallback.Log($"WriteToArchiveAsync (ReadMany): {sw1.ElapsedMilliseconds} ms.\n");

            if (documentsToArchive.StatusCode != HttpStatusCode.OK)
            {
                throw new CosmosException("ReadManyItemsAsync failed", documentsToArchive.StatusCode, -1, documentsToArchive.ActivityId, documentsToArchive.RequestCharge);
            }

            var sw2 = Stopwatch.StartNew();

            var archiveWriteTasks = documentsToArchive.Select(ArchiveDocumentAsync);
            await Task.WhenAll(archiveWriteTasks).ConfigureAwait(false);

            _logCallback.Log($"WriteToArchiveAsync (Write): {sw2.ElapsedMilliseconds} ms.\n");
        }

        public Task DeleteAsync(IEnumerable<Uuid> dataAvailableNotifications, string partitionKey)
        {
            var documentPartitionKey = new PartitionKey(partitionKey);
            var deleteTasks = dataAvailableNotifications
                .Select(dataAvailableNotification =>
                    _repositoryContainer.Container.DeleteItemStreamAsync(dataAvailableNotification.ToString(), documentPartitionKey)).ToList();

            return Task.WhenAll(deleteTasks);
        }

        private static async IAsyncEnumerable<DataAvailableNotification> ExecuteBatchAsync(IQueryable<CosmosDataAvailable> query)
        {
            const int batchSize = 10000;

            var batchStart = 0;
            bool canHaveMoreItems;

            do
            {
                var nextBatchQuery = query.Skip(batchStart).Take(batchSize);
                var returnedItems = 0;

                await foreach (var item in ExecuteQueryAsync(nextBatchQuery).ConfigureAwait(false))
                {
                    yield return item;
                    returnedItems++;
                }

                batchStart += batchSize;
                canHaveMoreItems = returnedItems == batchSize;
            }
            while (canHaveMoreItems);
        }

        private static async IAsyncEnumerable<DataAvailableNotification> ExecuteQueryAsync(IQueryable<CosmosDataAvailable> query)
        {
            await foreach (var document in query.AsCosmosIteratorAsync().ConfigureAwait(false))
            {
                yield return new DataAvailableNotification(
                    new Uuid(document.Id),
                    new MarketOperator(new GlobalLocationNumber(document.Recipient)),
                    new ContentType(document.ContentType),
                    Enum.Parse<DomainOrigin>(document.Origin, true),
                    new SupportsBundling(document.SupportsBundling),
                    new Weight(document.RelativeWeight),
                    new SequenceNumber(document.SequenceNumber),
                    document.PartitionKey);
            }
        }

        private static async IAsyncEnumerable<T> ExecuteQueryAsync<T>(IQueryable<T> query)
        {
            await foreach (var document in query.AsCosmosIteratorAsync().ConfigureAwait(false))
            {
                yield return document;
            }
        }

        private Task ArchiveDocumentAsync(CosmosDataAvailable documentToWrite)
        {
            return _repositoryContainer
                .ArchiveContainer.UpsertItemAsync(documentToWrite, new PartitionKey(documentToWrite.PartitionKey));
        }
    }
}
