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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.GridArea;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Organization;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.EntryPoint.Operations.Functions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.Operations
{
    [UnitTest]
    public sealed class MarketParticipantIngestionTests
    {
        [Fact]
        public async Task RunAsync_GridAreaUpdatedMessage_DoesNothing()
        {
            // Arrange
            var logger = new Mock<ILogger<MarketParticipantIngestionFunction>>().Object;
            var mediator = new Mock<IMediator>();
            var parser = new SharedIntegrationEventParser();

            var target = new MarketParticipantIngestionFunction(logger, mediator.Object, parser);

            var message = new GridAreaUpdatedIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "fake_value",
                "fake_value",
                PriceAreaCode.DK1,
                Guid.NewGuid());

            var bytes = new GridAreaUpdatedIntegrationEventParser().ParseToSharedIntegrationEvent(message);

            // Act
            await target.RunAsync(bytes).ConfigureAwait(false);

            // Assert
            mediator.Verify(m => m.Send(It.IsAny<UpdateActorCommand>(), CancellationToken.None), Times.Never);
            mediator.Verify(m => m.Send(It.IsAny<DeleteActorCommand>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task RunAsync_OrganizationUpdatedMessage_DoesNothing()
        {
            // Arrange
            var logger = new Mock<ILogger<MarketParticipantIngestionFunction>>().Object;
            var mediator = new Mock<IMediator>();
            var parser = new SharedIntegrationEventParser();

            var target = new MarketParticipantIngestionFunction(logger, mediator.Object, parser);

            var message = new OrganizationUpdatedIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "fake_value",
                "fake_value",
                new Address("fake_value", "fake_value", "fake_value", "fake_value", "fake_value"));

            var bytes = new OrganizationUpdatedIntegrationEventParser().ParseToSharedIntegrationEvent(message);

            // Act
            await target.RunAsync(bytes).ConfigureAwait(false);

            // Assert
            mediator.Verify(m => m.Send(It.IsAny<UpdateActorCommand>(), CancellationToken.None), Times.Never);
            mediator.Verify(m => m.Send(It.IsAny<DeleteActorCommand>(), CancellationToken.None), Times.Never);
        }

        [Theory]
        [InlineData(ActorStatus.New, false)]
        [InlineData(ActorStatus.Active, true)]
        [InlineData(ActorStatus.Passive, true)]
        [InlineData(ActorStatus.Inactive, false)]
        public async Task RunAsync_ActorUpdatedMessage_Status(ActorStatus status, bool isUpdate)
        {
            // Arrange
            var logger = new Mock<ILogger<MarketParticipantIngestionFunction>>().Object;
            var mediator = new Mock<IMediator>();
            var parser = new SharedIntegrationEventParser();

            var target = new MarketParticipantIngestionFunction(logger, mediator.Object, parser);

            var message = new ActorUpdatedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                new ActorNumber("fake_value", ActorNumberType.Unknown),
                status,
                Enumerable.Empty<BusinessRoleCode>(),
                Enumerable.Empty<ActorMarketRole>());

            var bytes = new ActorUpdatedIntegrationEventParser().ParseToSharedIntegrationEvent(message);

            // Act
            await target.RunAsync(bytes).ConfigureAwait(false);

            // Assert
            if (isUpdate)
            {
                mediator.Verify(m => m.Send(It.IsAny<UpdateActorCommand>(), CancellationToken.None), Times.Once);
                mediator.Verify(m => m.Send(It.IsAny<DeleteActorCommand>(), CancellationToken.None), Times.Never);
            }
            else
            {
                mediator.Verify(m => m.Send(It.IsAny<UpdateActorCommand>(), CancellationToken.None), Times.Never);
                mediator.Verify(m => m.Send(It.IsAny<DeleteActorCommand>(), CancellationToken.None), Times.Once);
            }
        }

        [Fact]
        public async Task RunAsync_ActorUpdatedNoExternalId_DeletesActor()
        {
            // Arrange
            var logger = new Mock<ILogger<MarketParticipantIngestionFunction>>().Object;
            var mediator = new Mock<IMediator>();
            var parser = new SharedIntegrationEventParser();

            var target = new MarketParticipantIngestionFunction(logger, mediator.Object, parser);

            var message = new ActorUpdatedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                new ActorNumber("fake_value", ActorNumberType.Unknown),
                ActorStatus.Active,
                Enumerable.Empty<BusinessRoleCode>(),
                Enumerable.Empty<ActorMarketRole>());

            var bytes = new ActorUpdatedIntegrationEventParser().ParseToSharedIntegrationEvent(message);

            // Act
            await target.RunAsync(bytes).ConfigureAwait(false);

            // Assert
            mediator.Verify(m => m.Send(It.IsAny<UpdateActorCommand>(), CancellationToken.None), Times.Never);
            mediator.Verify(m => m.Send(It.IsAny<DeleteActorCommand>(), CancellationToken.None), Times.Once);
        }
    }
}
