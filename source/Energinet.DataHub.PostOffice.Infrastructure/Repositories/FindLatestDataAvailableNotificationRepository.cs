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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Microsoft.Azure.Cosmos.Linq;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories;

public sealed class FindLatestDataAvailableNotificationRepository
{
    private readonly IDataAvailableNotificationRepositoryContainer _repositoryContainer;

    public FindLatestDataAvailableNotificationRepository(
        IDataAvailableNotificationRepositoryContainer repositoryContainer)
    {
        _repositoryContainer = repositoryContainer;
    }

    public async Task<(DataAvailableNotification? Notification, DateTime Timestamp, bool IsDequeued)> FindLatestDataAvailableNotificationAsync(
        ActorId recipient,
        DomainOrigin domain)
    {
        var asLinq = _repositoryContainer
            .Cabinet
            .GetItemLinqQueryable<CosmosDataAvailable>();

        var query =
            from dataAvailableNotification in asLinq
            where dataAvailableNotification.Recipient == recipient.Value.ToString()
            where dataAvailableNotification.Origin == domain.ToString()
            orderby dataAvailableNotification.Timestamp descending
            select dataAvailableNotification;

        var latestNotification = await query
            .Take(1)
            .AsCosmosIteratorAsync()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (latestNotification == null)
            return (null, DateTime.MinValue, false);

        var partitionKey = string.Join(
            '_',
            latestNotification.Recipient,
            latestNotification.Origin,
            latestNotification.ContentType);

        var drawerLinq = _repositoryContainer
            .Cabinet
            .GetItemLinqQueryable<CosmosCabinetDrawer>();

        var drawerQuery =
            from cabinetDrawer in drawerLinq
            where
                cabinetDrawer.PartitionKey == partitionKey &&
                cabinetDrawer.Id == latestNotification.PartitionKey
            select cabinetDrawer;

        var notificationDrawer = await drawerQuery
            .Take(1)
            .AsCosmosIteratorAsync()
            .SingleAsync()
            .ConfigureAwait(false);

        var itemsInDrawer = await CountItemsInDrawerAsync(notificationDrawer).ConfigureAwait(false);
        var isDequeued = itemsInDrawer == notificationDrawer.Position;

        var timestamp = DateTimeOffset.FromUnixTimeSeconds(latestNotification.Timestamp).DateTime;
        return (CosmosDataAvailableMapper.Map(latestNotification), timestamp, isDequeued);
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
}
