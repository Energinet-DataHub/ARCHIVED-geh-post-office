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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Handlers;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;
using DomainOrigin = Energinet.DataHub.PostOffice.Domain.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Tests.Handlers
{
    [UnitTest]
    public sealed class PeekHandlerTests
    {
        [Fact]
        public async Task PeekCommandHandle_NullArgument_ThrowsException()
        {
            // Arrange
            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle((PeekCommand)null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekCommandHandle_WithData_ReturnsDataStream()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString();
            var recipient = new ActorId(Guid.NewGuid());
            var request = new PeekCommand(recipient.Value.ToString(), bundleId, ResponseFormat.Json, 1.0);

            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(bundleId),
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object,
                Enumerable.Empty<string>(),
                ResponseFormat.Json);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        recipient,
                        It.Is<Uuid>(r => BundleIdCheck(r, request)),
                        ResponseFormat.Json,
                        1.0))
                .ReturnsAsync(bundle);

            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act
            var (hasContent, bid, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(hasContent);
            Assert.Equal(bundleId, bid);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekCommandHandle_WithoutData_ReturnsNullStream()
        {
            // Arrange
            var request = new PeekCommand("82144962-38A2-4293-B346-0D6336D1ABF4", Guid.NewGuid().ToString(), ResponseFormat.Json, 1.0);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var bundleId = It.Is<Uuid>(r => string.Equals(r.ToString(), request.BundleId, StringComparison.OrdinalIgnoreCase));

            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAsync(
                        new ActorId(Guid.Parse(request.MarketOperator)),
                        bundleId,
                        ResponseFormat.Json,
                        1.0))
                .ReturnsAsync((Bundle?)null);

            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act
            var (hasContent, _, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.False(hasContent);
            Assert.Equal(0, stream.Length);
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsCommandHandle_NullArgument_ThrowsException()
        {
            // Arrange
            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle((PeekAggregationsCommand)null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsCommandHandle_WithData_ReturnsDataStream()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString();
            var recipient = new ActorId(Guid.NewGuid());
            var request = new PeekAggregationsCommand(recipient.Value.ToString(), bundleId, ResponseFormat.Json, 1.0);

            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(bundleId),
                recipient,
                DomainOrigin.TimeSeries,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object,
                Enumerable.Empty<string>(),
                ResponseFormat.Json);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAggregationsAsync(
                        recipient,
                        It.Is<Uuid>(r => r.ToString().Equals(request.BundleId, StringComparison.OrdinalIgnoreCase)),
                        ResponseFormat.Json,
                        1.0))
                .ReturnsAsync(bundle);

            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act
            var (hasContent, bid, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(hasContent);
            Assert.Equal(bundleId, bid);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekAggregationsCommandHandle_WithoutData_ReturnsNullStream()
        {
            // Arrange
            var request = new PeekAggregationsCommand("0D0F2F39-1E4B-4C44-B160-40F5B41943F7", Guid.NewGuid().ToString(), ResponseFormat.Json, 1.0);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedAggregationsAsync(
                        new ActorId(Guid.Parse(request.MarketOperator)),
                        It.Is<Uuid>(r => string.Equals(r.ToString(), request.BundleId, StringComparison.OrdinalIgnoreCase)),
                        ResponseFormat.Json,
                        1.0))
                .ReturnsAsync((Bundle?)null);

            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act
            var (hasContent, _, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.False(hasContent);
            Assert.Equal(0, stream.Length);
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekTimeSeriesCommandHandle_NullArgument_ThrowsException()
        {
            // Arrange
            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle((PeekTimeSeriesCommand)null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekTimeSeriesCommandHandle_WithData_ReturnsDataStream()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString();
            var recipient = new ActorId(Guid.NewGuid());
            var request = new PeekTimeSeriesCommand(recipient.Value.ToString(), bundleId, ResponseFormat.Json, 1.0);

            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(bundleId),
                recipient,
                DomainOrigin.Charges,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object,
                Enumerable.Empty<string>(),
                ResponseFormat.Json);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedTimeSeriesAsync(
                        recipient,
                        It.Is<Uuid>(r => BundleIdCheck(r, request)),
                        ResponseFormat.Json,
                        1.0))
                .ReturnsAsync(bundle);

            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act
            var (hasContent, bid, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(hasContent);
            Assert.Equal(bundleId, bid);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekTimeSeriesCommandHandle_WithoutData_ReturnsNullStream()
        {
            // Arrange
            var request = new PeekTimeSeriesCommand("033C16BF-C1AB-42BE-B225-6700A208EFF6", Guid.NewGuid().ToString(), ResponseFormat.Json, 1.0);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedTimeSeriesAsync(
                        new ActorId(Guid.Parse(request.MarketOperator)),
                        It.Is<Uuid>(r => BundleIdCheck(r, request)),
                        ResponseFormat.Json,
                        1.0))
                .ReturnsAsync((Bundle?)null);

            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act
            var (hasContent, _, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.False(hasContent);
            Assert.Equal(0, stream.Length);
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekMasterDataCommandHandle_NullArgument_ThrowsException()
        {
            // Arrange
            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act + Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => target.Handle((PeekMasterDataCommand)null!, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekMasterDataCommandHandle_WithData_ReturnsDataStream()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString();
            var recipient = new ActorId(Guid.NewGuid());
            var request = new PeekMasterDataCommand(recipient.Value.ToString(), bundleId, ResponseFormat.Json, 1.0);

            var bundleContentMock = new Mock<IBundleContent>();
            bundleContentMock
                .Setup(x => x.OpenAsync())
                .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var bundle = new Bundle(
                new Uuid(bundleId),
                recipient,
                DomainOrigin.MarketRoles,
                new ContentType("fake_value"),
                Array.Empty<Uuid>(),
                bundleContentMock.Object,
                Enumerable.Empty<string>(),
                ResponseFormat.Json);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedMasterDataAsync(
                        recipient,
                        It.Is<Uuid>(r => BundleIdCheck(r, request)),
                        ResponseFormat.Json,
                        1.0))
                .ReturnsAsync(bundle);

            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act
            var (hasContent, bid, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(hasContent);
            Assert.Equal(bundleId, bid);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task PeekMasterDataCommandHandle_WithoutData_ReturnsNullStream()
        {
            // Arrange
            var request = new PeekMasterDataCommand("5C23584D-135C-46B8-86D8-B87CA41CE614", Guid.NewGuid().ToString(), ResponseFormat.Json, 1.0);

            var warehouseDomainServiceMock = new Mock<IMarketOperatorDataDomainService>();
            warehouseDomainServiceMock
                .Setup(x =>
                    x.GetNextUnacknowledgedMasterDataAsync(
                        It.Is<ActorId>(r =>
                            string.Equals(r.Value.ToString(), request.MarketOperator, StringComparison.OrdinalIgnoreCase)),
                        It.Is<Uuid>(r => BundleIdCheck(r, request)),
                        ResponseFormat.Json,
                        1.0))
                .ReturnsAsync((Bundle?)null);

            var target = new PeekHandler(
                warehouseDomainServiceMock.Object,
                new Mock<ILogger>().Object,
                new Mock<ICorrelationContext>().Object);

            // Act
            var (hasContent, _, stream, _) = await target.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.False(hasContent);
            Assert.Equal(0, stream.Length);
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        private static bool BundleIdCheck(Uuid r, PeekCommandBase request)
        {
            return r.ToString().Equals(request.BundleId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
