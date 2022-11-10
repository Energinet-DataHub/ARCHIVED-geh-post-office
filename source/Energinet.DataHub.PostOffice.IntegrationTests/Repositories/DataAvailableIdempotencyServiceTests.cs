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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Infrastructure.Services;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories;

[Collection("IntegrationTest")]
[IntegrationTest]
public sealed class DataAvailableIdempotencyServiceTests
{
    [Fact]
    public async Task CheckIdempotency_NewNotification_ReturnsFalse()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost.InitializeAsync();
        var scope = host.BeginScope();

        var dataAvailableIdempotencyService = scope.GetInstance<IDataAvailableIdempotencyService>();
        var dataAvailableNotificationRepositoryContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

        var cosmosCabinetDrawer = new CosmosCabinetDrawer
        {
            Id = Guid.NewGuid().ToString(),
            PartitionKey = Guid.NewGuid().ToString()
        };

        var notification = new DataAvailableNotification(
            new Uuid(),
            new ActorId(Guid.NewGuid()),
            new ContentType("fake_value"),
            DomainOrigin.Aggregations,
            new SupportsBundling(false),
            new Weight(1),
            new SequenceNumber(1),
            new DocumentType("fake_value"));

        await dataAvailableNotificationRepositoryContainer
            .Cabinet
            .CreateItemAsync(CosmosDataAvailableMapper.Map(notification, cosmosCabinetDrawer.Id));

        // Act
        var result = await dataAvailableIdempotencyService.CheckIdempotencyAsync(
            notification,
            cosmosCabinetDrawer);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckIdempotency_SameNotification_ReturnsTrue()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost.InitializeAsync();
        var scope = host.BeginScope();

        var dataAvailableIdempotencyService = scope.GetInstance<IDataAvailableIdempotencyService>();
        var dataAvailableNotificationRepositoryContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

        var cosmosCabinetDrawer = new CosmosCabinetDrawer
        {
            Id = Guid.NewGuid().ToString(),
            PartitionKey = Guid.NewGuid().ToString()
        };

        var notification = new DataAvailableNotification(
            new Uuid(),
            new ActorId(Guid.NewGuid()),
            new ContentType("fake_value"),
            DomainOrigin.Aggregations,
            new SupportsBundling(false),
            new Weight(1),
            new SequenceNumber(1),
            new DocumentType("fake_value"));

        await dataAvailableNotificationRepositoryContainer
            .Cabinet
            .CreateItemAsync(CosmosDataAvailableMapper.Map(notification, cosmosCabinetDrawer.Id));

        await dataAvailableIdempotencyService.CheckIdempotencyAsync(
            notification,
            cosmosCabinetDrawer);

        // Act
        var result = await dataAvailableIdempotencyService.CheckIdempotencyAsync(
            notification,
            cosmosCabinetDrawer);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckIdempotency_SameNotificationNotInDrawer_ReturnsFalse()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost.InitializeAsync();
        var scope = host.BeginScope();

        var dataAvailableIdempotencyService = scope.GetInstance<IDataAvailableIdempotencyService>();

        var cosmosCabinetDrawer = new CosmosCabinetDrawer
        {
            Id = Guid.NewGuid().ToString(),
            PartitionKey = Guid.NewGuid().ToString()
        };

        var notification = new DataAvailableNotification(
            new Uuid(),
            new ActorId(Guid.NewGuid()),
            new ContentType("fake_value"),
            DomainOrigin.Aggregations,
            new SupportsBundling(false),
            new Weight(1),
            new SequenceNumber(1),
            new DocumentType("fake_value"));

        await dataAvailableIdempotencyService.CheckIdempotencyAsync(
            notification,
            cosmosCabinetDrawer);

        // Act
        var result = await dataAvailableIdempotencyService.CheckIdempotencyAsync(
            notification,
            cosmosCabinetDrawer);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckIdempotency_SameNotificationDifferentContent_ThrowsException()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost.InitializeAsync();
        var scope = host.BeginScope();

        var dataAvailableIdempotencyService = scope.GetInstance<IDataAvailableIdempotencyService>();
        var dataAvailableNotificationRepositoryContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();

        var cosmosCabinetDrawer = new CosmosCabinetDrawer
        {
            Id = Guid.NewGuid().ToString(),
            PartitionKey = Guid.NewGuid().ToString()
        };

        var notification = new DataAvailableNotification(
            new Uuid(),
            new ActorId(Guid.NewGuid()),
            new ContentType("fake_value"),
            DomainOrigin.Aggregations,
            new SupportsBundling(false),
            new Weight(1),
            new SequenceNumber(1),
            new DocumentType("fake_value"));

        await dataAvailableNotificationRepositoryContainer
            .Cabinet
            .CreateItemAsync(CosmosDataAvailableMapper.Map(notification, cosmosCabinetDrawer.Id));

        await dataAvailableIdempotencyService.CheckIdempotencyAsync(
            notification,
            cosmosCabinetDrawer);

        // Act
        var sameIdDifferentContent = new DataAvailableNotification(
            notification.NotificationId,
            notification.Recipient,
            notification.ContentType,
            notification.Origin,
            notification.SupportsBundling,
            new Weight(8),
            notification.SequenceNumber,
            notification.DocumentType);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(() => dataAvailableIdempotencyService.CheckIdempotencyAsync(
             sameIdDifferentContent,
             cosmosCabinetDrawer));
    }
}
