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
    public sealed class PeekIntegrationTests
    {
        [Fact]
        public async Task PeekCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand("   ", "   ", ResponseFormat.Json, 1.0);

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(peekCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekCommand_InvalidFormat_ThrowsExceptionWithXmlFirst()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();

            await AddTimeSeriesNotificationAsync(recipientGuid).ConfigureAwait(false);
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand(recipientGuid, bundleId, ResponseFormat.Xml, 1.0);
            var peekCommandJson = new PeekTimeSeriesCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act + Assert
            await mediator.Send(peekCommand).ConfigureAwait(false);
            await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => mediator.Send(peekCommandJson)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekCommand_InvalidFormat_ThrowsExceptionWithJsonFirst()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();

            await AddTimeSeriesNotificationAsync(recipientGuid).ConfigureAwait(false);
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);
            var peekCommandXml = new PeekTimeSeriesCommand(recipientGuid, bundleId, ResponseFormat.Xml, 1.0);

            // Act + Assert
            await mediator.Send(peekCommand).ConfigureAwait(false);
            await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => mediator.Send(peekCommandXml)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekCommand_Empty_ReturnsNothing()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var unrelatedGln = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();

            await AddTimeSeriesNotificationAsync(unrelatedGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.HasContent);
        }

        [Fact]
        public async Task PeekCommand_SingleNotification_ReturnsData()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddTimeSeriesNotificationAsync(recipientGuid).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekCommand_SingleNotificationMultiplePeek_ReturnsData()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddTimeSeriesNotificationAsync(recipientGuid).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand(recipientGuid, bundleId, ResponseFormat.Xml, 1.0);

            // Act
            var responseA = await mediator.Send(peekCommand).ConfigureAwait(false);
            var responseB = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(responseA);
            Assert.True(responseA.HasContent);
            Assert.NotNull(responseB);
            Assert.True(responseB.HasContent);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        public async Task PeekCommand_NotificationCannotBundle_ReturnsFirstNotification(bool first, bool second, bool third)
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var expectedGuid = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, first).ConfigureAwait(false);
            var unexpectedGuidA = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, second).ConfigureAwait(false);
            var unexpectedGuidB = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, third).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            var dataAvailableNotificationIds = await bundleContents.GetDataAvailableIdsAsync(scope).ConfigureAwait(false);

            Assert.Single(dataAvailableNotificationIds);
            Assert.Contains(expectedGuid, dataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidA, dataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidB, dataAvailableNotificationIds);
        }

        [Fact]
        public async Task PeekCommand_AllNotificationsCanBundle_ReturnsBundle()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var expectedGuidA = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, true).ConfigureAwait(false);
            var expectedGuidB = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, true).ConfigureAwait(false);
            var expectedGuidC = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, true).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            var dataAvailableNotificationIds = await bundleContents.GetDataAvailableIdsAsync(scope).ConfigureAwait(false);

            Assert.Equal(3, dataAvailableNotificationIds.Count);
            Assert.Contains(expectedGuidA, dataAvailableNotificationIds);
            Assert.Contains(expectedGuidB, dataAvailableNotificationIds);
            Assert.Contains(expectedGuidC, dataAvailableNotificationIds);
        }

        [Fact]
        public async Task PeekTimeSeriesCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekTimeSeriesCommand("   ", "   ", ResponseFormat.Json, 1.0);

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(peekCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekTimeSeriesCommand_Empty_ReturnsNothing()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var unrelatedGln = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();

            await AddTimeSeriesNotificationAsync(unrelatedGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekTimeSeriesCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.HasContent);
        }

        [Fact]
        public async Task PeekTimeSeriesCommand_SingleNotification_ReturnsData()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();

            await AddTimeSeriesNotificationAsync(recipientGuid).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekTimeSeriesCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekTimeSeriesCommand_SingleNotificationMultiplePeek_ReturnsData()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();

            await AddTimeSeriesNotificationAsync(recipientGuid).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekTimeSeriesCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var responseA = await mediator.Send(peekCommand).ConfigureAwait(false);
            var responseB = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(responseA);
            Assert.True(responseA.HasContent);
            Assert.NotNull(responseB);
            Assert.True(responseB.HasContent);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        public async Task PeekTimeSeriesCommand_NotificationCannotBundle_ReturnsFirstNotification(bool first, bool second, bool third)
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var expectedGuid = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, first).ConfigureAwait(false);
            var unexpectedGuidA = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, second).ConfigureAwait(false);
            var unexpectedGuidB = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, third).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekTimeSeriesCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            var dataAvailableNotificationIds = await bundleContents.GetDataAvailableIdsAsync(scope).ConfigureAwait(false);

            Assert.Single(dataAvailableNotificationIds);
            Assert.Contains(expectedGuid, dataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidA, dataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidB, dataAvailableNotificationIds);
        }

        [Fact]
        public async Task PeekTimeSeriesCommand_AllNotificationsCanBundle_ReturnsBundle()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var expectedGuidA = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, true).ConfigureAwait(false);
            var expectedGuidB = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, true).ConfigureAwait(false);
            var expectedGuidC = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.TimeSeries, true).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekTimeSeriesCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            var dataAvailableNotificationIds = await bundleContents.GetDataAvailableIdsAsync(scope).ConfigureAwait(false);

            Assert.Equal(3, dataAvailableNotificationIds.Count);
            Assert.Contains(expectedGuidA, dataAvailableNotificationIds);
            Assert.Contains(expectedGuidB, dataAvailableNotificationIds);
            Assert.Contains(expectedGuidC, dataAvailableNotificationIds);
        }

        [Fact]
        public async Task PeekAggregationsCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsCommand("   ", "    ", ResponseFormat.Json, 1.0);

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(peekCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsCommand_Empty_ReturnsNothing()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var unrelatedGln = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();

            await AddAggregationsNotificationAsync(unrelatedGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.HasContent);
        }

        [Fact]
        public async Task PeekAggregationsCommand_SingleAggregationNotification_ReturnsData()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddAggregationsNotificationAsync(recipientGuid).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekAggregationsCommand_SingleNotificationMultiplePeek_ReturnsData()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddAggregationsNotificationAsync(recipientGuid).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var responseA = await mediator.Send(peekCommand).ConfigureAwait(false);
            var responseB = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(responseA);
            Assert.True(responseA.HasContent);
            Assert.NotNull(responseB);
            Assert.True(responseB.HasContent);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        public async Task PeekAggregationsCommand_NotificationCannotBundle_ReturnsFirstNotification(bool first, bool second, bool third)
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var expectedGuid = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.Wholesale, first).ConfigureAwait(false);
            var unexpectedGuidA = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.Wholesale, second).ConfigureAwait(false);
            var unexpectedGuidB = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.Wholesale, third).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            var dataAvailableNotificationIds = await bundleContents.GetDataAvailableIdsAsync(scope).ConfigureAwait(false);

            Assert.Single(dataAvailableNotificationIds);
            Assert.Contains(expectedGuid, dataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidA, dataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidB, dataAvailableNotificationIds);
        }

        [Fact]
        public async Task PeekAggregationsCommand_AllNotificationsCanBundle_ReturnsBundle()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var expectedGuidA = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.Wholesale, true).ConfigureAwait(false);
            var expectedGuidB = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.Wholesale, true).ConfigureAwait(false);
            var expectedGuidC = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.Wholesale, true).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekAggregationsCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            var dataAvailableNotificationIds = await bundleContents.GetDataAvailableIdsAsync(scope).ConfigureAwait(false);

            Assert.Equal(3, dataAvailableNotificationIds.Count);
            Assert.Contains(expectedGuidA, dataAvailableNotificationIds);
            Assert.Contains(expectedGuidB, dataAvailableNotificationIds);
            Assert.Contains(expectedGuidC, dataAvailableNotificationIds);
        }

        [Fact]
        public async Task PeekMasterDataCommand_InvalidCommand_ThrowsException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand("   ", "    ", ResponseFormat.Json, 1.0);

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(peekCommand)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekMasterDataCommand_Empty_ReturnsNothing()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var unrelatedGln = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();

            await AddMeteringPointsNotificationAsync(unrelatedGln).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_SingleMarketRolesNotification_ReturnsData()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddMarketRolesNotificationAsync(recipientGuid).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_SingleMeteringPointsNotification_ReturnsData()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddMeteringPointsNotificationAsync(recipientGuid).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_SingleChargesNotification_ReturnsData()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddChargesNotificationAsync(recipientGuid).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_SingleNotificationMultiplePeek_ReturnsData()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var bundleId = Guid.NewGuid().ToString();
            await AddMeteringPointsNotificationAsync(recipientGuid).ConfigureAwait(false);

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var responseA = await mediator.Send(peekCommand).ConfigureAwait(false);
            var responseB = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(responseA);
            Assert.True(responseA.HasContent);
            Assert.NotNull(responseB);
            Assert.True(responseB.HasContent);
        }

        [Fact]
        public async Task PeekMasterDataCommand_AllThreeNotifications_ReturnsOldest()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var expectedGuid = await AddMarketRolesNotificationAsync(recipientGuid).ConfigureAwait(false);
            var unexpectedGuidA = await AddMeteringPointsNotificationAsync(recipientGuid).ConfigureAwait(false);
            var unexpectedGuidB = await AddChargesNotificationAsync(recipientGuid).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            var dataAvailableNotificationIds = await bundleContents.GetDataAvailableIdsAsync(scope).ConfigureAwait(false);

            Assert.Contains(expectedGuid, dataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidA, dataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidB, dataAvailableNotificationIds);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        public async Task PeekMasterDataCommand_NotificationCannotBundle_ReturnsFirstNotification(bool first, bool second, bool third)
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var expectedGuid = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.MeteringPoints, first).ConfigureAwait(false);
            var unexpectedGuidA = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.MeteringPoints, second).ConfigureAwait(false);
            var unexpectedGuidB = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.MeteringPoints, third).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            var dataAvailableNotificationIds = await bundleContents.GetDataAvailableIdsAsync(scope).ConfigureAwait(false);

            Assert.Single(dataAvailableNotificationIds);
            Assert.Contains(expectedGuid, dataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidA, dataAvailableNotificationIds);
            Assert.DoesNotContain(unexpectedGuidB, dataAvailableNotificationIds);
        }

        [Fact]
        public async Task PeekMasterDataCommand_AllNotificationsCanBundle_ReturnsBundle()
        {
            // Arrange
            var recipientGuid = Guid.NewGuid().ToString();
            var expectedGuidA = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.MeteringPoints, true).ConfigureAwait(false);
            var expectedGuidB = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.MeteringPoints, true).ConfigureAwait(false);
            var expectedGuidC = await AddBundlingNotificationAsync(recipientGuid, DomainOrigin.MeteringPoints, true).ConfigureAwait(false);
            var bundleId = Guid.NewGuid().ToString();

            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var peekCommand = new PeekMasterDataCommand(recipientGuid, bundleId, ResponseFormat.Json, 1.0);

            // Act
            var response = await mediator.Send(peekCommand).ConfigureAwait(false);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.HasContent);

            var bundleContents = await response.Data.ReadAsDataBundleRequestAsync().ConfigureAwait(false);
            var dataAvailableNotificationIds = await bundleContents.GetDataAvailableIdsAsync(scope).ConfigureAwait(false);

            Assert.Equal(3, dataAvailableNotificationIds.Count);
            Assert.Contains(expectedGuidA, dataAvailableNotificationIds);
            Assert.Contains(expectedGuidB, dataAvailableNotificationIds);
            Assert.Contains(expectedGuidC, dataAvailableNotificationIds);
        }

        private static async Task AddTimeSeriesNotificationAsync(string recipientGuid)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationDto(
                dataAvailableUuid.ToString(),
                recipientGuid,
                "timeseries",
                DomainOrigin.TimeSeries,
                false,
                1,
                1,
                "RSM??");

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
        }

        private static async Task AddAggregationsNotificationAsync(string recipientGuid)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationDto(
                dataAvailableUuid.ToString(),
                recipientGuid,
                "aggregations",
                DomainOrigin.Wholesale,
                false,
                1,
                1,
                "RSM??");

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
        }

        private static async Task<Guid> AddMarketRolesNotificationAsync(string recipientGuid)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationDto(
                dataAvailableUuid.ToString(),
                recipientGuid,
                "marketroles",
                DomainOrigin.MarketRoles,
                false,
                1,
                1,
                "RSM??");

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
            return dataAvailableUuid;
        }

        private static async Task<Guid> AddMeteringPointsNotificationAsync(string recipientGuid)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationDto(
                dataAvailableUuid.ToString(),
                recipientGuid,
                "meteringpoints",
                DomainOrigin.MeteringPoints,
                false,
                1,
                1,
                "RSM??");

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
            return dataAvailableUuid;
        }

        private static async Task<Guid> AddChargesNotificationAsync(string recipientGuid)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationDto(
                dataAvailableUuid.ToString(),
                recipientGuid,
                "charges",
                DomainOrigin.Charges,
                false,
                1,
                1,
                "RSM??");

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
            return dataAvailableUuid;
        }

        private static async Task<Guid> AddBundlingNotificationAsync(string recipientGuid, DomainOrigin origin, bool supportsBundling)
        {
            var dataAvailableUuid = Guid.NewGuid();
            var dataAvailableCommand = new DataAvailableNotificationDto(
                dataAvailableUuid.ToString(),
                recipientGuid,
                "content_type",
                origin,
                supportsBundling,
                1,
                1,
                "RSM??");

            await AddNotificationAsync(dataAvailableCommand).ConfigureAwait(false);
            return dataAvailableUuid;
        }

        private static async Task AddNotificationAsync(DataAvailableNotificationDto dataAvailableDto)
        {
            await using var host = await SubDomainIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var mediator = scope.GetInstance<IMediator>();

            var command = new InsertDataAvailableNotificationsCommand(new[] { dataAvailableDto });
            await mediator.Send(command).ConfigureAwait(false);
        }
    }
}
