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
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Handlers
{
    [UnitTest]
    public sealed class DataAvailableNotificationCleanUpCommandHandlerTests
    {
        [Fact]
        public async Task Handle_NullArgument_ThrowsException()
        {
            // Arrange
            var repository = new Mock<IDataAvailableNotificationCleanUpRepository>();
            var target = new DataAvailableNotificationCleanUpCommandHandler(repository.Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle(null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Handle_RepositoryIsCalled()
        {
            // Arrange
            var repository = new Mock<IDataAvailableNotificationCleanUpRepository>();
            var target = new DataAvailableNotificationCleanUpCommandHandler(repository.Object);

            var request = new DataAvailableNotificationCleanUpCommand();

            // Act
            await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            repository.Verify(x => x.DeleteOldCabinetDrawersAsync(), times: Times.Once);
        }
    }
}
