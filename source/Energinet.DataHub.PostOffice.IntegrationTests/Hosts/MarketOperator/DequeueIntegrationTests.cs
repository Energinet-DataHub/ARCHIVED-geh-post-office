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
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.IntegrationTests.Common;
using FluentValidation;
using MediatR;
using Xunit;
using Xunit.Categories;
using DataAvailableNotificationDto = Energinet.DataHub.PostOffice.Application.Commands.DataAvailableNotificationDto;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Hosts.MarketOperator
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public sealed class DequeueIntegrationTests
    {
        [Fact]
        public async Task Dequeue_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var dequeueCommand = new DequeueCommand("  ", "  ");

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(dequeueCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Dequeue_NoData_ReturnsNotDequeued()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleUuid = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var dequeueCommand = new DequeueCommand(recipientGuid, bundleUuid);

            // Act
            var response = await mediator.Send(dequeueCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.IsDequeued);
        }

        [Fact]
        public async Task Dequeue_HasData_ReturnsIsDequeued()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddDataAvailableNotificationAsync(recipientGuid, DomainOrigin.TimeSeries).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekResponse = await mediator.Send(new PeekCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0)).ConfigureAwait(false);
            var bundleUuid = await ReadBundleIdAsync(peekResponse).ConfigureAwait(false);

            var dequeueCommand = new DequeueCommand(recipientGuid, bundleUuid);

            // Act
            var response = await mediator.Send(dequeueCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsDequeued);
        }

        [Fact]
        public async Task Dequeue_HasDataTwoDequeue_ReturnsNotDequeued()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddDataAvailableNotificationAsync(recipientGuid, DomainOrigin.TimeSeries).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekResponse = await mediator.Send(new PeekCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0)).ConfigureAwait(false);
            var bundleUuid = await ReadBundleIdAsync(peekResponse).ConfigureAwait(false);

            var dequeueCommand = new DequeueCommand(recipientGuid, bundleUuid);

            // Act
            var responseA = await mediator.Send(dequeueCommand).ConfigureAwait(false);
            var responseB = await mediator.Send(dequeueCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(responseA);
            Assert.True(responseA.IsDequeued);
            Assert.NotNull(responseB);
            Assert.False(responseB.IsDequeued);
        }

        [Fact]
        public async Task Dequeue_DifferentRecipients_ReturnsNotDequeued()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var unrelatedGln = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddDataAvailableNotificationAsync(recipientGuid, DomainOrigin.TimeSeries).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekResponse = await mediator.Send(new PeekCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0)).ConfigureAwait(false);
            var bundleUuid = await ReadBundleIdAsync(peekResponse).ConfigureAwait(false);

            var dequeueCommand = new DequeueCommand(unrelatedGln, bundleUuid);

            // Act
            var response = await mediator.Send(dequeueCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.IsDequeued);
        }

        [Fact]
        public async Task Dequeue_DifferentEndpointsForSameRecipient_CanDequeueFromAllEndpoints()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            await AddDataAvailableNotificationAsync(recipientGuid, DomainOrigin.Wholesale);
            await AddDataAvailableNotificationAsync(recipientGuid, DomainOrigin.Charges);
            await AddDataAvailableNotificationAsync(recipientGuid, DomainOrigin.TimeSeries);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var timeSeriesPeek = new PeekTimeSeriesCommand(recipientGuid, null, ResponseFormat.Xml, 1.0);
            var aggregationsPeek = new PeekAggregationsCommand(recipientGuid, null, ResponseFormat.Xml, 1.0);
            var masterDataPeek = new PeekMasterDataCommand(recipientGuid, null, ResponseFormat.Xml, 1.0);

            var timeSeriesPeekResponse = await mediator.Send(timeSeriesPeek);
            var aggregationsPeekResponse = await mediator.Send(aggregationsPeek);
            var masterDataPeekResponse = await mediator.Send(masterDataPeek);

            var timeSeriesBundleUuid = await ReadBundleIdAsync(timeSeriesPeekResponse);
            var aggregationsBundleUuid = await ReadBundleIdAsync(aggregationsPeekResponse);
            var masterDataBundleUuid = await ReadBundleIdAsync(masterDataPeekResponse);

            var timeSeriesDequeueCommand = new DequeueCommand(recipientGuid, timeSeriesBundleUuid);
            var aggregationsDequeueCommand = new DequeueCommand(recipientGuid, aggregationsBundleUuid);
            var masterDataDequeueCommand = new DequeueCommand(recipientGuid, masterDataBundleUuid);

            // Act
            // Order matters! The newest bundle must be dequeued first, otherwise the test may incorrectly succeed.
            var masterDataResponse = await mediator.Send(masterDataDequeueCommand);
            var aggregationsResponse = await mediator.Send(aggregationsDequeueCommand);
            var timeSeriesResponse = await mediator.Send(timeSeriesDequeueCommand);

            // Assert
            Assert.NotNull(masterDataResponse);
            Assert.NotNull(aggregationsResponse);
            Assert.NotNull(timeSeriesResponse);
            Assert.True(masterDataResponse.IsDequeued);
            Assert.True(aggregationsResponse.IsDequeued);
            Assert.True(timeSeriesResponse.IsDequeued);
        }

        private static async Task AddDataAvailableNotificationAsync(string recipientGuid, DomainOrigin origin)
        {
            var dataAvailableUuid = Guid.NewGuid().ToString();
            var dataAvailableNotification = new DataAvailableNotificationDto(
                dataAvailableUuid,
                recipientGuid,
                $"{origin}_content_type",
                origin,
                false,
                1,
                1,
                "RSM??");

            await using var host = await SubDomainIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var command = new InsertDataAvailableNotificationsCommand(new[] { dataAvailableNotification });
            await mediator.Send(command).ConfigureAwait(false);
        }

        private static async Task<string> ReadBundleIdAsync(PeekResponse response)
        {
            Assert.True(response.HasContent);
            var bundleContents = await response.Data
                .ReadAsDataBundleRequestAsync()
                .ConfigureAwait(false);

            var bundleId = bundleContents.IdempotencyId.Split("_", StringSplitOptions.RemoveEmptyEntries).First();
            return bundleId;
        }
    }
}
