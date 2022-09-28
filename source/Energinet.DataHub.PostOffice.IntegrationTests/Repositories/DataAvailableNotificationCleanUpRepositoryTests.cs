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
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Constants;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.IntegrationTests.Common;
using Microsoft.Azure.Cosmos;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;
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

            var futureDate8Days = SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(8);

            var mockedClock = new Mock<IClock>();
            mockedClock.Setup(l => l.GetCurrentInstant()).Returns(futureDate8Days);
            scope.Container!.Register<IClock>(() => mockedClock.Object);

            var dataAvailableNotificationRepositoryContainer = scope.GetInstance<IDataAvailableNotificationRepositoryContainer>();
            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var dataAvailableNotificationCleanUpRepository = scope.GetInstance<IDataAvailableNotificationCleanUpRepository>();

            var recipient = new LegacyActorId(new MockedGln());
            var partitionKey = string.Join('_', recipient.Value, DomainOrigin.Charges, "default_content_type");

            var notifications = CreateInfinite(recipient, 1).Take(1).ToList();

            // Act
            await dataAvailableNotificationRepository
                .SaveAsync(new CabinetKey(notifications[0]), notifications)
                .ConfigureAwait(false);

            var reader = await dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, DomainOrigin.Charges)
                .ConfigureAwait(false);

            var itemsBeforeCleanUp = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            var cabinetDrawerLinq = dataAvailableNotificationRepositoryContainer.Cabinet.GetItemLinqQueryable<CosmosCabinetDrawer>(allowSynchronousQueryExecution: true);
            var query =
                from cabinetDrawerCosmos in cabinetDrawerLinq
                where cabinetDrawerCosmos.PartitionKey == partitionKey
                select cabinetDrawerCosmos;

            var cabinetDrawerAdded = query.ToList().First();

            var replaceOperation = PatchOperation.Replace("/position", RepositoryConstants.MaximumCabinetDrawerItemCount);
            await dataAvailableNotificationRepositoryContainer
                .Cabinet
                .PatchItemAsync<CosmosCabinetDrawer>(cabinetDrawerAdded.Id, new PartitionKey(cabinetDrawerAdded.PartitionKey), new List<PatchOperation>() { replaceOperation });

            await dataAvailableNotificationCleanUpRepository
                .DeleteOldCabinetDrawersAsync()
                .ConfigureAwait(false);

            var itemsAfterCleanUp = await reader!
                .ReadToEndAsync()
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(reader);

            Assert.True(itemsBeforeCleanUp.Count == 1);
            Assert.True(itemsAfterCleanUp.Count == 0);
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
    }
}
