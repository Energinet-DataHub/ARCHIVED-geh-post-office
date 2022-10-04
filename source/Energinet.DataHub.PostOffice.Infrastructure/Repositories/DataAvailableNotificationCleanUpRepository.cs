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

using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Helpers;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public sealed class DataAvailableNotificationCleanUpRepository : IDataAvailableNotificationCleanUpRepository
    {
        private readonly IClock _systemClock;
        private readonly ILogger<DataAvailableNotificationCleanUpRepository> _logger;
        private readonly IDataAvailableNotificationRepositoryContainer _repositoryContainer;

        public DataAvailableNotificationCleanUpRepository(
            IDataAvailableNotificationRepositoryContainer repositoryContainer,
            IClock systemClock,
            ILogger<DataAvailableNotificationCleanUpRepository> logger)
        {
            _repositoryContainer = repositoryContainer;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task DeleteOldCabinetDrawersAsync()
        {
            var asLinq = _repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosCabinetDrawer>();

            var nowInIsoUtc = _systemClock.GetCurrentInstant() - Duration.FromDays(RepositoryConstants.DataAvailableNotificationDaysOldWhenDeleted);
            var deletionTimeFormatForDb = nowInIsoUtc.ToUnixTimeSeconds();

            var query =
                from cabinetDrawer in asLinq
                where
                    cabinetDrawer.Position == RepositoryConstants.MaximumCabinetDrawerItemCount &&
                    cabinetDrawer.TimeStamp < deletionTimeFormatForDb
                select cabinetDrawer;

            await foreach (var drawerToDelete in query.AsCosmosIteratorAsync())
            {
                await FindAndDeleteDataAvailableNotificationsInDrawerAsync(drawerToDelete.Id).ConfigureAwait(false);
                await DeleteDrawerAsync(drawerToDelete).ConfigureAwait(false);
            }
        }

        private async Task FindAndDeleteDataAvailableNotificationsInDrawerAsync(string dataAvailableNotificationPartitionKey)
        {
            try
            {
                var dataAvailableNotificationIterator = _repositoryContainer.Cabinet.GetItemQueryIterator<CosmosDataAvailable>(
                    new QueryDefinition("SELECT * FROM c"),
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(dataAvailableNotificationPartitionKey)
                    });

                while (dataAvailableNotificationIterator.HasMoreResults)
                {
                    var nextNotifications = await dataAvailableNotificationIterator.ReadNextAsync().ConfigureAwait(false);
                    foreach (var dataAvailable in nextNotifications)
                    {
                        await DeleteDataAvailableNotificationAsync(dataAvailable).ConfigureAwait(false);
                    }
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "DeleteDataAvailableNotificationsInDrawerAsync, ErrorCode: {StatusCode}", ex.StatusCode);
                throw;
            }
        }

        private async Task DeleteDataAvailableNotificationAsync(CosmosDataAvailable dataAvailable)
        {
            try
            {
                await _repositoryContainer.Catalog.DeleteItemAsync<CosmosDataAvailable>(dataAvailable.Id, new PartitionKey(dataAvailable.PartitionKey)).ConfigureAwait(false);
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "DeleteDataAvailableNotificationsInDrawerAsync, ErrorCode: {StatusCode}, DataAvailableId: {Id}", ex.StatusCode, dataAvailable.Id);
            }
        }

        private async Task DeleteDrawerAsync(CosmosCabinetDrawer drawerToDelete)
        {
            try
            {
                await _repositoryContainer
                    .Cabinet
                    .DeleteItemAsync<CosmosCabinetDrawer>(
                        drawerToDelete.Id,
                        new PartitionKey(drawerToDelete.PartitionKey))
                    .ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Concurrent calls may have removed the file.
                _logger.LogError(ex, "DeleteDrawerAsync, ErrorCode: {StatusCode}", ex.StatusCode);
            }
        }
    }
}
