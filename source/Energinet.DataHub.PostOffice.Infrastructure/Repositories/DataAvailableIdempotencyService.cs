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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Infrastructure.Services;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories;

public sealed class DataAvailableIdempotencyService : IDataAvailableIdempotencyService
{
    private readonly IDataAvailableNotificationRepositoryContainer _repositoryContainer;

    public DataAvailableIdempotencyService(IDataAvailableNotificationRepositoryContainer repositoryContainer)
    {
        _repositoryContainer = repositoryContainer;
    }

    async Task<bool> IDataAvailableIdempotencyService.WasReceivedPreviouslyAsync(
        DataAvailableNotification notification,
        CosmosCabinetDrawer destinationDrawer)
    {
        var documentId = CreateDocumentId(notification);
        var partitionKey = CreateUniformPartitionKey(notification);

        var uniqueId = new CosmosUniqueId
        {
            Id = documentId,
            PartitionKey = partitionKey,
            Content = ToBase64Content(notification),
            DrawerId = destinationDrawer.Id
        };

        try
        {
            await _repositoryContainer
                .Idempotency
                .CreateItemAsync(uniqueId)
                .ConfigureAwait(false);

            return false;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            var conflictingResponse = await _repositoryContainer
                .Idempotency
                .ReadItemAsync<CosmosUniqueId>(
                    uniqueId.Id,
                    new PartitionKey(uniqueId.PartitionKey))
                .ConfigureAwait(false);

            var existingUniqueId = conflictingResponse.Resource;

            using var conflictingItem = await _repositoryContainer
                .Cabinet
                .ReadItemStreamAsync(
                    existingUniqueId.Id,
                    new PartitionKey(existingUniqueId.DrawerId))
                .ConfigureAwait(false);

            if (conflictingItem.StatusCode == HttpStatusCode.NotFound)
            {
                await _repositoryContainer
                    .Idempotency
                    .UpsertItemAsync(uniqueId)
                    .ConfigureAwait(false);

                return false;
            }

            conflictingItem.EnsureSuccessStatusCode();

            if (string.Equals(existingUniqueId.Content, uniqueId.Content, StringComparison.Ordinal))
            {
                return true;
            }

            throw new ValidationException($"Idempotency check failed for DataAvailable {documentId}.", ex);
        }
    }

    private static string CreateDocumentId(DataAvailableNotification notification)
    {
        return notification.NotificationId.ToString();
    }

    private static string CreateUniformPartitionKey(DataAvailableNotification notification)
    {
        return notification.NotificationId
            .AsGuid()
            .ToByteArray()[0]
            .ToString(CultureInfo.InvariantCulture);
    }

    private static string ToBase64Content(DataAvailableNotification notification)
    {
        using var ms = new MemoryStream();
        ms.Write(Encoding.UTF8.GetBytes(notification.ContentType.Value));
        ms.Write(BitConverter.GetBytes((int)notification.Origin));
        ms.Write(Encoding.UTF8.GetBytes(notification.Recipient.Value));
        ms.Write(BitConverter.GetBytes(notification.SupportsBundling.Value));
        ms.Write(BitConverter.GetBytes(notification.Weight.Value));
        return Convert.ToBase64String(ms.ToArray());
    }
}
