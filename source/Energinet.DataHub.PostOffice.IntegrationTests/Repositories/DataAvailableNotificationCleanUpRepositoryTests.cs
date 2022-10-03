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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Helpers;
using Energinet.DataHub.PostOffice.IntegrationTests.Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Moq;
using NodaTime;
using SimpleInjector;
using Xunit;
using Xunit.Categories;
using Container = Microsoft.Azure.Cosmos.Container;
using DomainOrigin = Energinet.DataHub.PostOffice.Domain.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public sealed class DataAvailableNotificationCleanUpRepositoryTests
    {
        [Fact]
        public async Task DataAvailableNotificationCleanUp_RemovesNotificationAndDrawer()
        {
            // Arrange
            await using var host = await OperationsIntegrationHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            SetUpMockClock(scope, GetClock366DaysInFuture());

            var dataAvailableNotificationRepositoryContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();
            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var dataAvailableNotificationCleanUpRepository = scope.GetInstance<IDataAvailableNotificationCleanUpRepository>();

            var recipient = new LegacyActorId(new MockedGln());
            var partitionKey = string.Join('_', recipient.Value, DomainOrigin.Charges, "default_content_type");

            var notifications = CreateInfinite(recipient, 1).Take(1).ToList();

            // Act *****

            // Save DataAvailable and build cabinet, with timestamp from today
            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(notifications[0]), notifications)
                .ConfigureAwait(false);

            // Get reader for next unacknowledged notifications
            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Charges)
                .ConfigureAwait(false);

            // Item count before cleanup
            var itemsBeforeCleanUp = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            var cabinetDrawerAdded = await ReadCabinetByPartitionKey(dataAvailableNotificationRepositoryContainer.Cabinet, partitionKey)
                .AsCosmosIteratorAsync()
                .FirstOrDefaultAsync().ConfigureAwait(false);

            // Update cabinet to be at position 10000, that indicates a full partition/drawer
            var replaceOperation = PatchOperation.Replace("/position", RepositoryConstants.MaximumCabinetDrawerItemCount);
            await dataAvailableNotificationRepositoryContainer
                .Cabinet
                .PatchItemAsync<CosmosCabinetDrawer>(cabinetDrawerAdded!.Id, new PartitionKey(cabinetDrawerAdded.PartitionKey), new List<PatchOperation>() { replaceOperation });

            // Now remove cabinet and notifications in it
            await dataAvailableNotificationCleanUpRepository
                .DeleteOldCabinetDrawersAsync()
                .ConfigureAwait(false);

            // read items after cleanup
            var itemsAfterCleanUp = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            // Assert *****
            Assert.NotNull(reader);
            Assert.NotNull(cabinetDrawerAdded);

            Assert.True(itemsBeforeCleanUp.Count == 1);
            Assert.True(itemsAfterCleanUp.Count == 0);
        }

        [Fact]
        public async Task DataAvailableNotificationCleanUp_DontRemove_CleanUpDateNotReached()
        {
            // Arrange
            await using var host = await OperationsIntegrationHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            SetUpMockClock(scope, GetClock300DaysInFuture());

            var dataAvailableNotificationRepositoryContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();
            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var dataAvailableNotificationCleanUpRepository = scope.GetInstance<IDataAvailableNotificationCleanUpRepository>();

            var recipient = new LegacyActorId(new MockedGln());
            var partitionKey = string.Join('_', recipient.Value, DomainOrigin.Charges, "default_content_type");

            var notifications = CreateInfinite(recipient, 1).Take(1).ToList();

            // Act *****

            // Save DataAvailable and build cabinet, with timestamp from today
            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(notifications[0]), notifications)
                .ConfigureAwait(false);

            // Get reader for next unacknowledged notifications
            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Charges)
                .ConfigureAwait(false);

            // Item count before cleanup
            var itemsBeforeCleanUp = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            var cabinetDrawerAdded = await ReadCabinetByPartitionKey(dataAvailableNotificationRepositoryContainer.Cabinet, partitionKey)
                .AsCosmosIteratorAsync()
                .FirstOrDefaultAsync().ConfigureAwait(false);

            // Update cabinet to be at position 10000, that indicates a full partition/drawer
            var replaceOperation = PatchOperation.Replace("/position", RepositoryConstants.MaximumCabinetDrawerItemCount);
            await dataAvailableNotificationRepositoryContainer
                .Cabinet
                .PatchItemAsync<CosmosCabinetDrawer>(cabinetDrawerAdded!.Id, new PartitionKey(cabinetDrawerAdded.PartitionKey), new List<PatchOperation>() { replaceOperation });

            // Now remove cabinet and notifications in it
            await dataAvailableNotificationCleanUpRepository
                .DeleteOldCabinetDrawersAsync()
                .ConfigureAwait(false);

            // read items after cleanup
            var itemsInDrawerAfterCleanUp = await CountItemsInDrawerAsync(dataAvailableNotificationRepositoryContainer, cabinetDrawerAdded).ConfigureAwait(false);

            // Assert *****
            Assert.NotNull(reader);
            Assert.NotNull(cabinetDrawerAdded);

            Assert.True(itemsBeforeCleanUp.Count == 1);
            Assert.True(itemsInDrawerAfterCleanUp == 1);
        }

        [Fact]
        public async Task DataAvailableNotificationCleanUp_DontRemove_CleanUpDateReachedButNotAcknowledged()
        {
            // Arrange
            await using var host = await OperationsIntegrationHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            SetUpMockClock(scope, GetClock400DaysInFuture());

            var dataAvailableNotificationRepositoryContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();
            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var dataAvailableNotificationCleanUpRepository = scope.GetInstance<IDataAvailableNotificationCleanUpRepository>();

            var recipient = new LegacyActorId(new MockedGln());
            var partitionKey = string.Join('_', recipient.Value, DomainOrigin.Charges, "default_content_type");

            var notifications = CreateInfinite(recipient, 1).Take(1).ToList();

            // Act *****

            // Save DataAvailable and build cabinet, with timestamp from today
            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(notifications[0]), notifications)
                .ConfigureAwait(false);

            // Get reader for next unacknowledged notifications
            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Charges)
                .ConfigureAwait(false);

            // Item count before cleanup
            var itemsBeforeCleanUp = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            var cabinetDrawerAdded = await ReadCabinetByPartitionKey(dataAvailableNotificationRepositoryContainer.Cabinet, partitionKey)
                .AsCosmosIteratorAsync()
                .FirstOrDefaultAsync().ConfigureAwait(false);

            // Update cabinet to be at position 10000, that indicates a full partition/drawer
            var replaceOperation = PatchOperation.Replace("/position", RepositoryConstants.MaximumCabinetDrawerItemCount - 1);
            await dataAvailableNotificationRepositoryContainer
                .Cabinet
                .PatchItemAsync<CosmosCabinetDrawer>(cabinetDrawerAdded!.Id, new PartitionKey(cabinetDrawerAdded.PartitionKey), new List<PatchOperation>() { replaceOperation });

            // Now remove cabinet and notifications in it
            await dataAvailableNotificationCleanUpRepository
                .DeleteOldCabinetDrawersAsync()
                .ConfigureAwait(false);

            // read items after cleanup
            var itemsInDrawerAfterCleanUp = await CountItemsInDrawerAsync(dataAvailableNotificationRepositoryContainer, cabinetDrawerAdded).ConfigureAwait(false);

            // Assert *****
            Assert.NotNull(reader);
            Assert.NotNull(cabinetDrawerAdded);

            Assert.True(itemsBeforeCleanUp.Count == 1);
            Assert.True(itemsInDrawerAfterCleanUp == 1);
        }

        private static IQueryable<CosmosCabinetDrawer> ReadCabinetByPartitionKey(Container cabinetContainerReference, string partitionKey)
        {
            var cabinetDrawerLinq = cabinetContainerReference.GetItemLinqQueryable<CosmosCabinetDrawer>();
            var query =
                from cabinetDrawerCosmos in cabinetDrawerLinq
                where cabinetDrawerCosmos.PartitionKey == partitionKey
                select cabinetDrawerCosmos;

            return query;
        }

        private static void SetUpMockClock(Scope scope, Instant clockAdjust)
        {
            var mockedClock = new Mock<IClock>();
            mockedClock.Setup(l => l.GetCurrentInstant()).Returns(clockAdjust);
            scope.Container!.Register<IClock>(() => mockedClock.Object);
        }

        private static async Task<int> CountItemsInDrawerAsync(IDataAvailableNotificationRepositoryContainer repositoryContainer, CosmosCabinetDrawer drawer)
        {
            var asLinq = repositoryContainer
                .Cabinet
                .GetItemLinqQueryable<CosmosDataAvailable>();

            var query =
                from dataAvailable in asLinq
                where dataAvailable.PartitionKey == drawer.Id
                select dataAvailable;

            return await query.CountAsync().ConfigureAwait(false);
        }

        private static IEnumerable<DataAvailableNotification> CreateInfinite(
            ActorId recipient,
            long initialSequenceNumber,
            DomainOrigin domainOrigin = DomainOrigin.Charges,
            string contentType = "default_content_type",
            bool supportsBundling = true,
            int weight = 1)
        {
            while (true)
            {
                yield return new DataAvailableNotification(
                    new Uuid(Guid.NewGuid()),
                    recipient,
                    new ContentType(contentType),
                    domainOrigin,
                    new SupportsBundling(supportsBundling),
                    new Weight(weight),
                    new SequenceNumber(initialSequenceNumber++),
                    new DocumentType("RSM??"));
            }
        }

        private static Instant GetClock300DaysInFuture() => SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(300);
        private static Instant GetClock366DaysInFuture() => SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(366);
        private static Instant GetClock400DaysInFuture() => SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(400);
    }
}
