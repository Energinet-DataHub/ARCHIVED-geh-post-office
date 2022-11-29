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
using Energinet.DataHub.MessageHub.Core.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Moq;
using Xunit;
using Xunit.Categories;
using DomainOrigin = Energinet.DataHub.PostOffice.Domain.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Tests.Repositories
{
    [UnitTest]
    public sealed class BundleRepositoryTests
    {
        [Fact]
        public async Task GetNextUnacknowledgedAsync_NullRecipient_ThrowsException()
        {
            // Arrange
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var marketOperatorDataStorageService = new Mock<IMarketOperatorDataStorageService>();
            var storageHandler = new Mock<IStorageHandler>();
            var target = new BundleRepository(
                storageHandler.Object,
                bundleRepositoryContainer.Object,
                marketOperatorDataStorageService.Object,
                new Mock<IMarketOperatorFlowLogger>().Object);

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
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var marketOperatorDataStorageService = new Mock<IMarketOperatorDataStorageService>();
            var storageHandler = new Mock<IStorageHandler>();
            var marketOperator = new ActorId(Guid.NewGuid());
            var target = new BundleRepository(
                storageHandler.Object,
                bundleRepositoryContainer.Object,
                marketOperatorDataStorageService.Object,
                new Mock<IMarketOperatorFlowLogger>().Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.GetNextUnacknowledgedAsync(marketOperator, null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task AcknowledgeAsync_NullRecipient_ThrowsException()
        {
            // Arrange
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var marketOperatorDataStorageService = new Mock<IMarketOperatorDataStorageService>();
            var storageHandler = new Mock<IStorageHandler>();
            var target = new BundleRepository(
                storageHandler.Object,
                bundleRepositoryContainer.Object,
                marketOperatorDataStorageService.Object,
                new Mock<IMarketOperatorFlowLogger>().Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.AcknowledgeAsync(null!, new Uuid("E6C4A3CA-D49A-4F09-929A-605C403DEEB9")))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task AcknowledgeAsync_NullBundleId_ThrowsException()
        {
            // Arrange
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var marketOperatorDataStorageService = new Mock<IMarketOperatorDataStorageService>();
            var storageHandler = new Mock<IStorageHandler>();
            var target = new BundleRepository(
                storageHandler.Object,
                bundleRepositoryContainer.Object,
                marketOperatorDataStorageService.Object,
                new Mock<IMarketOperatorFlowLogger>().Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.AcknowledgeAsync(new ActorId(Guid.NewGuid()), null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SaveAsync_NullArgument_ThrowsException()
        {
            // Arrange
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var marketOperatorDataStorageService = new Mock<IMarketOperatorDataStorageService>();
            var storageHandler = new Mock<IStorageHandler>();
            var target = new BundleRepository(
                storageHandler.Object,
                bundleRepositoryContainer.Object,
                marketOperatorDataStorageService.Object,
                new Mock<IMarketOperatorFlowLogger>().Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.SaveAsync(null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task TryAddNextUnacknowledgedAsync_NullBundle_ThrowsException()
        {
            // Arrange
            var cabinetReader = new Mock<ICabinetReader>();
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var marketOperatorDataStorageService = new Mock<IMarketOperatorDataStorageService>();
            var storageHandler = new Mock<IStorageHandler>();
            var target = new BundleRepository(
                storageHandler.Object,
                bundleRepositoryContainer.Object,
                marketOperatorDataStorageService.Object,
                new Mock<IMarketOperatorFlowLogger>().Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.TryAddNextUnacknowledgedAsync(null!, cabinetReader.Object))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task TryAddNextUnacknowledgedAsync_NullReader_ThrowsException()
        {
            // Arrange
            var bundleRepositoryContainer = new Mock<IBundleRepositoryContainer>();
            var marketOperatorDataStorageService = new Mock<IMarketOperatorDataStorageService>();
            var storageHandler = new Mock<IStorageHandler>();
            var target = new BundleRepository(
                storageHandler.Object,
                bundleRepositoryContainer.Object,
                marketOperatorDataStorageService.Object,
                new Mock<IMarketOperatorFlowLogger>().Object);

            var bundle = new Bundle(
                new Uuid(Guid.NewGuid()),
                new ActorId(Guid.NewGuid()),
                DomainOrigin.Aggregations,
                null!,
                Array.Empty<Uuid>(),
                Enumerable.Empty<string>(),
                ResponseFormat.Json);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    target.TryAddNextUnacknowledgedAsync(bundle, null!))
                .ConfigureAwait(false);
        }
    }
}
