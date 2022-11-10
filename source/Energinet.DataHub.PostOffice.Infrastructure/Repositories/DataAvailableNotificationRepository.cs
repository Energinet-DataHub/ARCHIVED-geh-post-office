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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Helpers;
using Energinet.DataHub.PostOffice.Infrastructure.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public sealed class DataAvailableNotificationRepository : IDataAvailableNotificationRepository
    {
        private readonly IDataAvailableNotificationRepositoryContainer _repositoryContainer;
        private readonly IDataAvailableIdempotencyService _dataAvailableIdempotencyService;
        private readonly IBundleRepositoryContainer _bundleRepositoryContainer;
        private readonly ISequenceNumberRepository _sequenceNumberRepository;
        private readonly IMarketOperatorFlowLogger _marketOperatorFlowLogger;

        public DataAvailableNotificationRepository(
            IBundleRepositoryContainer bundleRepositoryContainer,
            IDataAvailableNotificationRepositoryContainer repositoryContainer,
            IDataAvailableIdempotencyService dataAvailableIdempotencyService,
            ISequenceNumberRepository sequenceNumberRepository,
            IMarketOperatorFlowLogger marketOperatorFlowLogger)
        {
            _bundleRepositoryContainer = bundleRepositoryContainer;
            _repositoryContainer = repositoryContainer;
            _dataAvailableIdempotencyService = dataAvailableIdempotencyService;
            _sequenceNumberRepository = sequenceNumberRepository;
            _marketOperatorFlowLogger = marketOperatorFlowLogger;
        }

        public async Task SaveAsync(CabinetKey key, IReadOnlyList<DataAvailableNotification> notifications)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(notifications, nameof(notifications));

            var nextDrawer = await FindExistingDrawerWithFreeSpaceAsync(key).ConfigureAwait(false);
            var nextDrawerItemCount = nextDrawer != null
                ? await CountItemsInDrawerAsync(nextDrawer).ConfigureAwait(false)
                : 0;

            for (var i = 0; i < notifications.Count; i++)
            {
                var notification = notifications[i];

                Debug.Assert(key == new CabinetKey(notification), "All notifications should belong to the provided key.");

                if (nextDrawer is null)
                {
                    nextDrawer = CreateEmptyDrawer(notification);
                    nextDrawerItemCount = 0;

                    await _repositoryContainer
                        .Cabinet
                        .CreateItemAsync(nextDrawer)
                        .ConfigureAwait(false);
                }

                if (await _dataAvailableIdempotencyService
                        .CheckIdempotencyAsync(notification, nextDrawer)
                        .ConfigureAwait(false))
                {
                    continue;
                }

                var cosmosDataAvailable = CosmosDataAvailableMapper.Map(notification, nextDrawer.Id);
                await _repositoryContainer
                    .Cabinet
                    .CreateItemAsync(cosmosDataAvailable)
                    .ConfigureAwait(false);

                nextDrawer = await GetUpdatedDrawerAsync(nextDrawer).ConfigureAwait(false);

                if (nextDrawer.Position == nextDrawerItemCount)
                {
                    var catalogEntry = CreateCatalogEntry(notification);
                    await _repositoryContainer
                        .Catalog
                        .UpsertItemAsync(catalogEntry)
                        .ConfigureAwait(false);
                }

                var itemsLeft = notifications.Count - (i + 1);
                var spaceLeft = RepositoryConstants.MaximumCabinetDrawerItemCount - (nextDrawerItemCount + 1);

                var itemsToFill = notifications
                    .Skip(i + 1)
                    .Take(Math.Min(itemsLeft, spaceLeft));

                var itemsInserted = await FillCabinetDrawerAsync(nextDrawer, itemsToFill).ConfigureAwait(false);
                i += itemsInserted;
                nextDrawerItemCount += itemsInserted;
                nextDrawerItemCount++;

                Debug.Assert(nextDrawerItemCount <= RepositoryConstants.MaximumCabinetDrawerItemCount, "Too many items were inserted into a single drawer.");

                if (nextDrawerItemCount == RepositoryConstants.MaximumCabinetDrawerItemCount)
                {
                    nextDrawer = null;
                }
            }
        }

        public async Task<ICabinetReader?> GetNextUnacknowledgedAsync(ActorId recipient, params DomainOrigin[] domains)
        {
            ArgumentNullException.ThrowIfNull(recipient, nameof(recipient));
            ArgumentNullException.ThrowIfNull(domains, nameof(domains));

            if (domains.Length == 0)
                domains = Enum.GetValues<DomainOrigin>();

            var entryLookups = domains
                .Where(domain => domain != DomainOrigin.Unknown)
                .Select(domain =>
                {
                    var asLinq = _repositoryContainer
                        .Catalog
                        .GetItemLinqQueryable<CosmosCatalogEntry>();

                    var partitionKey = string.Join('_', recipient.Value, domain);

                    var query =
                        from catalogEntry in asLinq
                        where catalogEntry.PartitionKey == partitionKey
                        orderby catalogEntry.NextSequenceNumber
                        select catalogEntry;

                    var task = query
                        .Take(1)
                        .AsCosmosIteratorAsync()
                        .FirstOrDefaultAsync();

                    return new { domain, task };
                });

            var smallestDomain = DomainOrigin.Unknown;
            CosmosCatalogEntry? smallestEntry = null;

            foreach (var entryLookup in entryLookups.ToList())
            {
                var catalogEntry = await entryLookup.task.ConfigureAwait(false);
                if (catalogEntry == null)
                {
                    await _marketOperatorFlowLogger
                        .LogNoCatalogWasFoundForDomainAsync(recipient, entryLookup.domain)
                        .ConfigureAwait(false);

                    continue;
                }

                await _marketOperatorFlowLogger
                    .LogCatalogWasFoundForDomainAsync(recipient, entryLookup.domain)
                    .ConfigureAwait(false);

                if (smallestEntry == null || smallestEntry.NextSequenceNumber > catalogEntry.NextSequenceNumber)
                {
                    smallestEntry = catalogEntry;
                    smallestDomain = entryLookup.domain;
                }
            }

            var maximumSequenceNumber = await _sequenceNumberRepository
                .GetMaximumSequenceNumberAsync()
                .ConfigureAwait(false);

            if (smallestEntry == null || smallestEntry.NextSequenceNumber > maximumSequenceNumber.Value)
            {
                await _marketOperatorFlowLogger
                    .LogLatestDataAvailableNotificationsAsync(recipient, domains)
                    .ConfigureAwait(false);

                return null;
            }

            var cabinetKey = new CabinetKey(
                recipient,
                smallestDomain,
                new ContentType(smallestEntry.ContentType));

            var cabinetReader = await GetCabinetReaderAsync(cabinetKey).ConfigureAwait(false);
            if (cabinetReader.CanPeek)
            {
                var nextItem = cabinetReader.Peek();
                if (nextItem.SequenceNumber.Value == smallestEntry.NextSequenceNumber)
                    return cabinetReader;
            }

            // If SaveAsync fails to insert DataAvailable, for example due to crash/timeout,
            // there will be a catalog entry pointing to a non-existent item.
            await DeleteOldCatalogEntryAsync(smallestEntry).ConfigureAwait(false);

            await _marketOperatorFlowLogger
                .LogLatestDataAvailableNotificationsAsync(recipient, domains)
                .ConfigureAwait(false);

            return null;
        }

        public async Task AcknowledgeAsync(Bundle bundle)
        {
            ArgumentNullException.ThrowIfNull(bundle, nameof(bundle));

            var asLinq = _bundleRepositoryContainer
                .Container
                .GetItemLinqQueryable<CosmosBundleDocument>();

            var recipient = bundle.Recipient.Value;
            var bundleId = bundle.BundleId.ToString();

            var query =
                from cosmosBundle in asLinq
                where cosmosBundle.Recipient == recipient &&
                      cosmosBundle.Id == bundleId
                select cosmosBundle;

            var fetchedBundle = await query
                .AsCosmosIteratorAsync()
                .SingleAsync()
                .ConfigureAwait(false);

            var updateTasks = fetchedBundle
                .AffectedDrawers
                .Select(async changes =>
                {
                    // This must happen first, before UpdateCatalogAsync.
                    // When SaveAsync checks the drawer, the changes are not
                    // yet known and so the catalog entry will be missing.
                    // When acknowledging, update the drawer, so that:
                    // a) If an item is inserted before UpdateDrawerAsync,
                    //    a catalog entry will be added in UpdateCatalogAsync.
                    // b) If an item is inserted after UpdateDrawerAsync,
                    //    a catalog entry will be added in UpdateCatalogAsync AND SaveAsync.
                    // c) If an item is inserted after UpdateDrawerAsync and UpdateCatalogAsync,
                    //    a catalog entry will be added in SaveAsync, if necessary.
                    await UpdateDrawerAsync(changes).ConfigureAwait(false);
                    await Task
                        .WhenAll(
                            UpdateCatalogAsync(fetchedBundle, changes),
                            DeleteOldCatalogEntryAsync(fetchedBundle, changes))
                        .ConfigureAwait(false);
                });

            await Task.WhenAll(updateTasks).ConfigureAwait(false);
        }

        private static CosmosCatalogEntry CreateCatalogEntry(DataAvailableNotification notification)
        {
            var partitionKey = string.Join(
                '_',
                notification.Recipient.Value,
                notification.Origin);

            return new CosmosCatalogEntry
            {
                Id = Guid.NewGuid().ToString(),
                ContentType = notification.ContentType.Value,
                NextSequenceNumber = notification.SequenceNumber.Value,
                PartitionKey = partitionKey
            };
        }

        private static CosmosCabinetDrawer CreateEmptyDrawer(DataAvailableNotification notification)
        {
            var partitionKey = string.Join(
                '_',
                notification.Recipient.Value,
                notification.Origin,
                notification.ContentType.Value);

            return new CosmosCabinetDrawer
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = partitionKey,
                OrderBy = notification.SequenceNumber.Value,
                Position = 0
            };
        }

        private Task<CosmosCabinetDrawer?> FindExistingDrawerWithFreeSpaceAsync(CabinetKey cabinetKey)
        {
            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosCabinetDrawer>();

            var partitionKey = string.Join(
                '_',
                cabinetKey.Recipient.Value,
                cabinetKey.Origin,
                cabinetKey.ContentType.Value);

            var query =
                from cabinetDrawer in asLinq
                where
                    cabinetDrawer.PartitionKey == partitionKey &&
                    cabinetDrawer.Position < RepositoryConstants.MaximumCabinetDrawerItemCount
                orderby cabinetDrawer.OrderBy descending
                select cabinetDrawer;

            return query
                .AsCosmosIteratorAsync()
                .FirstOrDefaultAsync();
        }

        private Task<CosmosCabinetDrawer> GetUpdatedDrawerAsync(CosmosCabinetDrawer drawer)
        {
            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosCabinetDrawer>();

            var query =
                from cabinetDrawer in asLinq
                where
                    cabinetDrawer.Id == drawer.Id &&
                    cabinetDrawer.PartitionKey == drawer.PartitionKey
                select cabinetDrawer;

            return query
                .AsCosmosIteratorAsync()
                .SingleAsync();
        }

        private async Task<int> CountItemsInDrawerAsync(CosmosCabinetDrawer drawer)
        {
            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailable in asLinq
                where dataAvailable.PartitionKey == drawer.Id
                select dataAvailable;

            return await query.CountAsync().ConfigureAwait(false);
        }

        private async Task<int> FillCabinetDrawerAsync(
            CosmosCabinetDrawer drawer,
            IEnumerable<DataAvailableNotification> notifications)
        {
            var count = 0;
            var tasks = notifications.Select(async x =>
            {
                if (await _dataAvailableIdempotencyService
                        .CheckIdempotencyAsync(x, drawer)
                        .ConfigureAwait(false))
                {
                    return;
                }

                var cosmosDataAvailable = CosmosDataAvailableMapper.Map(x, drawer.Id);
                await _repositoryContainer
                    .Cabinet
                    .CreateItemAsync(cosmosDataAvailable)
                    .ConfigureAwait(false);

                Interlocked.Increment(ref count);
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return count;
        }

        private async Task<ICabinetReader> GetCabinetReaderAsync(CabinetKey cabinetKey)
        {
            var drawers = new List<CosmosCabinetDrawer>();
            var content = new List<Task<IEnumerable<CosmosDataAvailable>>>();

            await foreach (var drawer in GetCabinetDrawersAsync(cabinetKey).ConfigureAwait(false))
            {
                var drawerContent = GetCabinetDrawerContentsAsync(drawer);
                drawers.Add(drawer);
                content.Add(drawerContent);
            }

            var cabinetReader = new AsyncCabinetReader(cabinetKey, drawers, content);
            await cabinetReader.InitializeAsync().ConfigureAwait(false);
            return cabinetReader;
        }

        private IAsyncEnumerable<CosmosCabinetDrawer> GetCabinetDrawersAsync(CabinetKey cabinetKey)
        {
            var partitionKey = string.Join(
                '_',
                cabinetKey.Recipient.Value,
                cabinetKey.Origin,
                cabinetKey.ContentType.Value);

            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosCabinetDrawer>();

            var query =
                from cabinetDrawer in asLinq
                where
                    cabinetDrawer.PartitionKey == partitionKey &&
                    cabinetDrawer.Position < RepositoryConstants.MaximumCabinetDrawerItemCount
                orderby cabinetDrawer.OrderBy
                select cabinetDrawer;

            return query.Take(RepositoryConstants.MaximumCabinetDrawersInRequest).AsCosmosIteratorAsync();
        }

        private async Task<IEnumerable<CosmosDataAvailable>> GetCabinetDrawerContentsAsync(CosmosCabinetDrawer drawer)
        {
            var maximumSequenceNumber = await _sequenceNumberRepository
                .GetMaximumSequenceNumberAsync()
                .ConfigureAwait(false);

            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailableNotification in asLinq
                where
                    dataAvailableNotification.PartitionKey == drawer.Id &&
                    dataAvailableNotification.SequenceNumber <= maximumSequenceNumber.Value
                orderby dataAvailableNotification.SequenceNumber
                select dataAvailableNotification;

            return await query
                .Skip(drawer.Position)
                .AsCosmosIteratorAsync()
                .ToListAsync()
                .ConfigureAwait(false);
        }

        private async Task UpdateDrawerAsync(CosmosCabinetDrawerChanges changes)
        {
            var options = new ItemRequestOptions
            {
                IfMatchEtag = changes.UpdatedDrawer.ETag
            };

            var updatedDrawer = changes.UpdatedDrawer;

            try
            {
                await _repositoryContainer
                     .Cabinet
                     .ReplaceItemAsync(updatedDrawer, updatedDrawer.Id, requestOptions: options)
                     .ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                // When two Acknowledge are executing, they must not overwrite each others values.
                // The failed ReplaceItemAsync is discarded.
            }
        }

        private async Task UpdateCatalogAsync(CosmosBundleDocument bundle, CosmosCabinetDrawerChanges changes)
        {
            var updatedCatalogEntry = changes.UpdatedCatalogEntry;
            if (updatedCatalogEntry != null)
            {
                await _repositoryContainer
                    .Catalog
                    .UpsertItemAsync(updatedCatalogEntry)
                    .ConfigureAwait(false);

                return;
            }

            if (changes.UpdatedDrawer.Position == RepositoryConstants.MaximumCabinetDrawerItemCount)
                return;

            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailable in asLinq
                where dataAvailable.PartitionKey == changes.UpdatedDrawer.Id
                orderby dataAvailable.SequenceNumber
                select (long?)dataAvailable.SequenceNumber;

            var nextSequenceNumber = await query
                .Skip(changes.UpdatedDrawer.Position)
                .Take(1)
                .AsCosmosIteratorAsync()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (!nextSequenceNumber.HasValue)
                return;

            var partitionKey = string.Join('_', bundle.Recipient, bundle.Origin);
            var contentType = bundle.ContentType;

            var nextCatalogEntry = new CosmosCatalogEntry
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = partitionKey,
                ContentType = contentType,
                NextSequenceNumber = nextSequenceNumber.Value,
            };

            await _repositoryContainer
                .Catalog
                .UpsertItemAsync(nextCatalogEntry)
                .ConfigureAwait(false);
        }

        private async Task DeleteOldCatalogEntryAsync(CosmosBundleDocument bundle, CosmosCabinetDrawerChanges changes)
        {
            var asLinq = _repositoryContainer
                .Catalog
                .GetItemLinqQueryable<CosmosCatalogEntry>();

            var partitionKey = string.Join('_', bundle.Recipient, bundle.Origin);
            var contentType = bundle.ContentType;

            var query =
                from catalogEntry in asLinq
                where catalogEntry.PartitionKey == partitionKey &&
                      catalogEntry.ContentType == contentType &&
                      catalogEntry.NextSequenceNumber == changes.InitialCatalogEntrySequenceNumber
                select catalogEntry;

            await foreach (var entry in query.AsCosmosIteratorAsync())
            {
                await DeleteOldCatalogEntryAsync(entry).ConfigureAwait(false);
            }
        }

        private async Task DeleteOldCatalogEntryAsync(CosmosCatalogEntry entry)
        {
            try
            {
                await _repositoryContainer
                    .Catalog
                    .DeleteItemAsync<CosmosCatalogEntry>(entry.Id, new PartitionKey(entry.PartitionKey))
                    .ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Concurrent calls may have removed the file.
            }
        }
    }
}
