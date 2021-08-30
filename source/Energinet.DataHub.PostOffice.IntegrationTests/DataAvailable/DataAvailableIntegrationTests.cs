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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.DataAvailable;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.DataAvailable
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public class DataAvailableIntegrationTests
    {
        [Fact]
        public async Task Test_DataAvailable_Integration_Create()
        {
            // Arrange
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();
            var dataAvailableCommand = GetDataAvailableCommand();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            // Act
            var result = await mediator.Send(dataAvailableCommand, CancellationToken.None).ConfigureAwait(false);
            var dataAvailablePeekResult = await dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(new MarketOperator(dataAvailableCommand.Recipient)).ConfigureAwait(false);

            // Assert
            dataAvailablePeekResult.Should().NotBeNull();
            dataAvailablePeekResult?.Recipient.Value.Should().Be(dataAvailableCommand.Recipient);
        }

        [Fact]
        public async Task Test_DataAvailable_Integration_PeekByMessageType()
        {
            // Arrange
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();
            var dataAvailableCommand = GetDataAvailableCommand();

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();
            var recipient = new MarketOperator(dataAvailableCommand.Recipient);
            var messageType = new ContentType(1, dataAvailableCommand.MessageType);

            // Act
            var result = await mediator.Send(dataAvailableCommand, CancellationToken.None).ConfigureAwait(false);
            var dataAvailablePeekResult = await dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(recipient, messageType).ConfigureAwait(false);

            // Assert
            dataAvailablePeekResult.Should().NotBeNullOrEmpty();
            dataAvailablePeekResult?.Should().Contain(e => messageType.Type.Equals(e.ContentType.Type, StringComparison.Ordinal));
        }

        [Fact]
        public async Task Test_DataAvailable_Integration_Dequeue()
        {
            // Arrange
            await using var host = await InboundIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();
            var dataAvailableCommand = GetDataAvailableCommand();
            var dequeueUuids = new List<Uuid> { new(dataAvailableCommand.UUID) };

            var dataAvailableNotificationRepository = scope.GetInstance<IDataAvailableNotificationRepository>();

            // Act
            var result = await mediator.Send(dataAvailableCommand, CancellationToken.None).ConfigureAwait(false);
            var dataAvailablePeekResult = await dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(new MarketOperator(dataAvailableCommand.Recipient)).ConfigureAwait(false);
            await dataAvailableNotificationRepository.AcknowledgeAsync(dequeueUuids).ConfigureAwait(false);
            var dataAvailablePeekDequeuedResult = await dataAvailableNotificationRepository.GetNextUnacknowledgedAsync(new MarketOperator(dataAvailableCommand.Recipient)).ConfigureAwait(false);

            // Assert
            dataAvailablePeekResult.Should().NotBeNull();
            dataAvailablePeekResult?.Recipient.Value.Should().Be(dataAvailableCommand.Recipient);
            dataAvailablePeekDequeuedResult.Should().BeNull();
        }

        private static DataAvailableCommand GetDataAvailableCommand()
        {
            return new(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "address-change",
                SubDomain.Charges.ToString(),
                false,
                1);
        }
    }
}
