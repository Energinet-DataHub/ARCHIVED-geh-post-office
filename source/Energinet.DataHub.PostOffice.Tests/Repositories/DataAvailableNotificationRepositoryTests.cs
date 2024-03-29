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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Energinet.DataHub.PostOffice.Infrastructure.Services;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Repositories
{
    [UnitTest]
    public sealed class DataAvailableNotificationRepositoryTests
    {
        [Fact]
        public async Task GetNextUnacknowledgedAsync_NullRecipient_ThrowsException()
        {
            // Arrange
            var target = CreateTarget();

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.GetNextUnacknowledgedAsync(null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task GetNextUnacknowledgedAsync_NullDomains_ThrowsException()
        {
            // Arrange
            var target = CreateTarget();

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.GetNextUnacknowledgedAsync(new ActorId(Guid.NewGuid()), null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SaveAsync_NullKey_ThrowsException()
        {
            // Arrange
            var target = CreateTarget();

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.SaveAsync(null!, Array.Empty<DataAvailableNotification>()))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SaveAsync_NullNotifications_ThrowsException()
        {
            // Arrange
            var target = CreateTarget();
            var cabinetKey = new CabinetKey(
                new ActorId(Guid.NewGuid()),
                DomainOrigin.Charges,
                new ContentType("fake_value"));

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.SaveAsync(cabinetKey, null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task AcknowledgeAsync_NullBundle_ThrowsException()
        {
            // Arrange
            var target = CreateTarget();

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.AcknowledgeAsync(null!))
                .ConfigureAwait(false);
        }

        private static DataAvailableNotificationRepository CreateTarget()
        {
            var dataAvailableNotificationRepositoryContainer = new Mock<IDataAvailableNotificationRepositoryContainer>();
            var dataAvailableIdempotencyService = new Mock<IDataAvailableIdempotencyService>();
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var sequenceNumberRepository = new Mock<ISequenceNumberRepository>();
            return new DataAvailableNotificationRepository(
                bundleRepositoryContainer.Object,
                dataAvailableNotificationRepositoryContainer.Object,
                dataAvailableIdempotencyService.Object,
                sequenceNumberRepository.Object,
                new Mock<IMarketOperatorFlowLogger>().Object);
        }
    }
}
