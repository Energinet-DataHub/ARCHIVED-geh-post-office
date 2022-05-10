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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Handlers;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Handlers;

[UnitTest]
public sealed class ActorUpdatedHandlerTests
{
    [Fact]
    public async Task UpdateActorCommandHandle_NullArgument_ThrowsException()
    {
        // Arrange
        var repository = new Mock<IActorRepository>();
        var target = new ActorUpdatedHandler(repository.Object, new Mock<ILogger>().Object);

        // Act + Assert
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => target.Handle((UpdateActorCommand)null!, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task DeleteActorCommandHandle_NullArgument_ThrowsException()
    {
        // Arrange
        var repository = new Mock<IActorRepository>();
        var target = new ActorUpdatedHandler(repository.Object, new Mock<ILogger>().Object);

        // Act + Assert
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => target.Handle((DeleteActorCommand)null!, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task UpdateActorCommandHandle_WithActor_SavesActor()
    {
        // Arrange
        var repository = new Mock<IActorRepository>();
        var target = new ActorUpdatedHandler(repository.Object, new Mock<ILogger>().Object);

        var actorId = Guid.NewGuid();
        var externalId = Guid.NewGuid();

        var updateActorCommand = new UpdateActorCommand(actorId.ToString(), externalId.ToString());

        // Act
        await target.Handle(updateActorCommand, CancellationToken.None).ConfigureAwait(false);

        // Assert
        repository.Verify(
            r => r.AddOrUpdateAsync(It.Is<Actor>(actor => actor.Id.Value == actorId.ToString() && actor.ExternalId.Value == externalId)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteActorCommandHandle_WithActor_DeletesActor()
    {
        // Arrange
        var repository = new Mock<IActorRepository>();
        var target = new ActorUpdatedHandler(repository.Object, new Mock<ILogger>().Object);

        var actorId = new ActorId(Guid.NewGuid());
        var externalId = new ExternalActorId(Guid.NewGuid());
        var actor = new Actor(actorId, externalId);

        repository
            .Setup(r => r.GetActorAsync(It.Is<ActorId>(id => id == actorId)))
            .ReturnsAsync(actor);

        var deleteActorCommand = new DeleteActorCommand(actorId.Value);

        // Act
        await target.Handle(deleteActorCommand, CancellationToken.None).ConfigureAwait(false);

        // Assert
        repository.Verify(r => r.DeleteAsync(actor), Times.Once);
    }

    [Fact]
    public async Task DeleteActorCommandHandle_WithoutActor_DoesNothing()
    {
        // Arrange
        var repository = new Mock<IActorRepository>();
        var target = new ActorUpdatedHandler(repository.Object, new Mock<ILogger>().Object);

        var actorId = new ActorId(Guid.NewGuid());

        repository
            .Setup(r => r.GetActorAsync(It.Is<ActorId>(id => id == actorId)))
            .ReturnsAsync((Actor?)null);

        var deleteActorCommand = new DeleteActorCommand(actorId.Value);

        // Act + Assert
        await target.Handle(deleteActorCommand, CancellationToken.None).ConfigureAwait(false);
    }
}
