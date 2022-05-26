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
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Repositories;

[UnitTest]
public sealed class ActorRepositoryTests
{
    [Fact]
    public async Task GetActorAsync_NullActorId_ThrowsException()
    {
        // Arrange
        var container = new Mock<IActorRepositoryContainer>();
        var target = new ActorRepository(container.Object);

        // Act + Assert
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => target.GetActorAsync((ActorId)null!))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task GetActorAsync_NullExternalActorId_ThrowsException()
    {
        // Arrange
        var container = new Mock<IActorRepositoryContainer>();
        var target = new ActorRepository(container.Object);

        // Act + Assert
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => target.GetActorAsync((ExternalActorId)null!))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task AddOrUpdateAsync_NullActor_ThrowsException()
    {
        // Arrange
        var container = new Mock<IActorRepositoryContainer>();
        var target = new ActorRepository(container.Object);

        // Act + Assert
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => target.AddOrUpdateAsync(null!))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task DeleteAsync_NullActor_ThrowsException()
    {
        // Arrange
        var container = new Mock<IActorRepositoryContainer>();
        var target = new ActorRepository(container.Object);

        // Act + Assert
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => target.DeleteAsync(null!))
            .ConfigureAwait(false);
    }
}
