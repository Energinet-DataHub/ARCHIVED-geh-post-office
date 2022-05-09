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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories;

[Collection("IntegrationTest")]
[IntegrationTest]
public sealed class ActorRepositoryIntegrationTests
{
    [Fact]
    public async Task GetActorAsync_GivenExternalActorId_ReturnsActor()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
        var scope = host.BeginScope();

        var container = scope.GetInstance<IActorRepositoryContainer>();
        var target = new ActorRepository(container);

        var actorId = Guid.NewGuid();
        var externalId = Guid.NewGuid();

        var actor = new Actor(new ActorId(actorId), new ExternalActorId(externalId));
        await target.SaveAsync(actor).ConfigureAwait(false);

        // Act
        var actual = await target
            .GetActorAsync(new ExternalActorId(externalId))
            .ConfigureAwait(false);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(actorId, Guid.Parse(actual!.Id.Value));
        Assert.Equal(externalId, actual.ExternalId.Value);
    }

    [Fact]
    public async Task GetActorAsync_NoActor_ReturnsNull()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
        var scope = host.BeginScope();

        var container = scope.GetInstance<IActorRepositoryContainer>();
        var target = new ActorRepository(container);

        var externalId = Guid.NewGuid();

        // Act
        var actual = await target
            .GetActorAsync(new ExternalActorId(externalId))
            .ConfigureAwait(false);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task SaveAsync_GivenActor_CanBeReadBack()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
        var scope = host.BeginScope();

        var container = scope.GetInstance<IActorRepositoryContainer>();
        var target = new ActorRepository(container);

        var actorId = Guid.NewGuid();
        var externalId = Guid.NewGuid();

        var actor = new Actor(new ActorId(actorId), new ExternalActorId(externalId));

        // Act
        await target.SaveAsync(actor).ConfigureAwait(false);

        // Assert
        var actual = await target
            .GetActorAsync(new ExternalActorId(externalId))
            .ConfigureAwait(false);

        Assert.NotNull(actual);
        Assert.Equal(actorId, Guid.Parse(actual!.Id.Value));
        Assert.Equal(externalId, actual.ExternalId.Value);
    }

    [Fact]
    public async Task DeleteAsync_GivenActor_IsDeleted()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
        var scope = host.BeginScope();

        var container = scope.GetInstance<IActorRepositoryContainer>();
        var target = new ActorRepository(container);

        var actorId = Guid.NewGuid();
        var externalId = Guid.NewGuid();

        var actor = new Actor(new ActorId(actorId), new ExternalActorId(externalId));
        await target.SaveAsync(actor).ConfigureAwait(false);

        // Act
        await target.DeleteAsync(actor).ConfigureAwait(false);

        // Assert
        var actual = await target
            .GetActorAsync(new ExternalActorId(externalId))
            .ConfigureAwait(false);

        Assert.Null(actual);
    }

    [Fact]
    public async Task DeleteAsync_NoActor_DoesNothing()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
        var scope = host.BeginScope();

        var container = scope.GetInstance<IActorRepositoryContainer>();
        var target = new ActorRepository(container);

        var actorId = Guid.NewGuid();
        var externalId = Guid.NewGuid();

        var actor = new Actor(new ActorId(actorId), new ExternalActorId(externalId));

        // Act + Assert
        await target.DeleteAsync(actor).ConfigureAwait(false);
    }
}
