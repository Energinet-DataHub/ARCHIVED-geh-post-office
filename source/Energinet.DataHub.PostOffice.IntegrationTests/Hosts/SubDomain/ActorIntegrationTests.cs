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
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using FluentValidation;
using MediatR;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Hosts.SubDomain;

[Collection("IntegrationTest")]
[IntegrationTest]
public sealed class ActorIntegrationTests
{
    [Fact]
    public async Task UpdateActorCommand_InvalidCommand_ThrowsException()
    {
        // Arrange
        const string blankValue = "  ";

        await using var host = await SubDomainIntegrationTestHost
            .InitializeAsync()
            .ConfigureAwait(false);

        await using var scope = host.BeginScope();
        var mediator = scope.GetInstance<IMediator>();

        var command = new UpdateActorCommand(blankValue, blankValue);

        // Act + Assert
        await Assert
            .ThrowsAsync<ValidationException>(() => mediator.Send(command))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task DeleteActorCommand_InvalidCommand_ThrowsException()
    {
        // Arrange
        const string blankValue = "  ";

        await using var host = await SubDomainIntegrationTestHost
            .InitializeAsync()
            .ConfigureAwait(false);

        await using var scope = host.BeginScope();
        var mediator = scope.GetInstance<IMediator>();

        var command = new DeleteActorCommand(blankValue);

        // Act + Assert
        await Assert
            .ThrowsAsync<ValidationException>(() => mediator.Send(command))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task UpdateActorCommand_NewActor_InsertsActor()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost
            .InitializeAsync()
            .ConfigureAwait(false);

        await using var scope = host.BeginScope();
        var mediator = scope.GetInstance<IMediator>();

        var actorId = Guid.NewGuid();
        var externalId = Guid.NewGuid();

        var command = new UpdateActorCommand(actorId.ToString(), externalId.ToString());

        // Act
        await mediator.Send(command).ConfigureAwait(false);

        // Assert
        var actorRepository = scope.GetInstance<IActorRepository>();

        var actualUsingId = await actorRepository.GetActorAsync(new ActorId(actorId)).ConfigureAwait(false);
        Assert.NotNull(actualUsingId);

        var actualUsingExternalId = await actorRepository.GetActorAsync(new ExternalActorId(externalId)).ConfigureAwait(false);
        Assert.NotNull(actualUsingExternalId);
    }

    [Fact]
    public async Task UpdateActorCommand_ExistingActor_UpdateActor()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost
            .InitializeAsync()
            .ConfigureAwait(false);

        await using var scope = host.BeginScope();
        var mediator = scope.GetInstance<IMediator>();

        var actorId = Guid.NewGuid();
        var externalId = Guid.NewGuid();
        var updatedId = Guid.NewGuid();

        var initialCommand = new UpdateActorCommand(actorId.ToString(), externalId.ToString());
        await mediator.Send(initialCommand).ConfigureAwait(false);

        var command = new UpdateActorCommand(actorId.ToString(), updatedId.ToString());

        // Act
        await mediator.Send(command).ConfigureAwait(false);

        // Assert
        var actorRepository = scope.GetInstance<IActorRepository>();

        var actualUsingId = await actorRepository.GetActorAsync(new ActorId(actorId)).ConfigureAwait(false);
        Assert.NotNull(actualUsingId);
        Assert.Equal(updatedId, actualUsingId!.ExternalId.Value);

        var actualUsingExternalId = await actorRepository.GetActorAsync(new ExternalActorId(updatedId)).ConfigureAwait(false);
        Assert.NotNull(actualUsingExternalId);
    }

    [Fact]
    public async Task DeleteActorCommand_HasActor_DeletesActor()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost
            .InitializeAsync()
            .ConfigureAwait(false);

        await using var scope = host.BeginScope();
        var mediator = scope.GetInstance<IMediator>();

        var actorId = Guid.NewGuid();
        var externalId = Guid.NewGuid();

        var initialCommand = new UpdateActorCommand(actorId.ToString(), externalId.ToString());
        await mediator.Send(initialCommand).ConfigureAwait(false);

        var command = new DeleteActorCommand(actorId.ToString());

        // Act
        await mediator.Send(command).ConfigureAwait(false);

        // Assert
        var actorRepository = scope.GetInstance<IActorRepository>();

        var actualUsingId = await actorRepository.GetActorAsync(new ActorId(actorId)).ConfigureAwait(false);
        Assert.Null(actualUsingId);

        var actualUsingExternalId = await actorRepository.GetActorAsync(new ExternalActorId(externalId)).ConfigureAwait(false);
        Assert.Null(actualUsingExternalId);
    }

    [Fact]
    public async Task DeleteActorCommand_NoActor_DoesNothing()
    {
        // Arrange
        await using var host = await SubDomainIntegrationTestHost
            .InitializeAsync()
            .ConfigureAwait(false);

        await using var scope = host.BeginScope();
        var mediator = scope.GetInstance<IMediator>();

        var actorId = Guid.NewGuid();
        var command = new DeleteActorCommand(actorId.ToString());

        // Act + Assert
        await mediator.Send(command).ConfigureAwait(false);
    }
}
