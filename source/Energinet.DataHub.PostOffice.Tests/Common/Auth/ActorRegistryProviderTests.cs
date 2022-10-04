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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.Auth;

[UnitTest]
public sealed class ActorRegistryProviderTests
{
    [Fact]
    public async Task GetActorAsync_WhenActorFound_ReturnsActorAndLogsActorFound()
    {
        // Arrange
        var repository = new Mock<IActorRepository>();
        var flowLogger = new Mock<IMarketOperatorFlowLogger>();
        var target = new ActorRegistryProvider(repository.Object, flowLogger.Object);

        var externalActorId = Guid.NewGuid();
        var internalActorId = Guid.NewGuid();

        repository
            .Setup(repo => repo.GetActorAsync(It.Is<ExternalActorId>(externalId => externalId.Value == externalActorId)))
            .ReturnsAsync(new Actor(new ActorId(internalActorId), new ExternalActorId(externalActorId)));

        // Act
        var actual = await target.GetActorAsync(externalActorId).ConfigureAwait(false);

        // Assert
        Assert.Equal(internalActorId, actual.ActorId);
        flowLogger.Verify(x => x.LogActorFoundAsync(externalActorId, internalActorId), Times.Exactly(1));
    }

    [Fact]
    public async Task GetActorAsync_WhenNoActorFound_ThrowsExceptionAndLogsNotFound()
    {
        // Arrange
        var repository = new Mock<IActorRepository>();
        var flowLogger = new Mock<IMarketOperatorFlowLogger>();
        var target = new ActorRegistryProvider(repository.Object, flowLogger.Object);

        var externalActorId = Guid.NewGuid();

        // Act + Assert
        await Assert
            .ThrowsAsync<InvalidOperationException>(() => target.GetActorAsync(externalActorId))
            .ConfigureAwait(false);
        flowLogger.Verify(x => x.LogActorNotFoundAsync(externalActorId), Times.Exactly(1));
    }
}
