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
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions.Helpers;
using Energinet.DataHub.PostOffice.Tests.Common.Auth;
using FluentAssertions;
using MediatR;
using Microsoft.Azure.Functions.Isolated.TestDoubles;
using Moq;
using Xunit;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.MarketOperator
{
    public class BundledIdTests
    {
        private const ResponseFormat ResponseFormat = MessageHub.Model.Model.ResponseFormat.Json;
        private const double ResponseVersion = 1.0;

        [Fact]
        public async Task PeekAggregations_WithContent_SetsBundledIdHeader()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString("N");
            Uri path = new($"https://localhost?{Constants.BundleIdQueryName}={bundleId}&{Constants.ResponseFormatQueryName}={ResponseVersion}&{Constants.ResponseVersionQueryName}={ResponseFormat}");

            var request = MockHelpers.CreateHttpRequestData(url: path);
            var mediator = new Mock<IMediator>();
            mediator
                .Setup(p => p.Send(It.IsAny<PeekAggregationsCommand>(), default))
                .ReturnsAsync(new PeekResponse(true, bundleId, Stream.Null, Enumerable.Empty<string>()));
            var identifier = new MockedMarketOperatorIdentity("fake_value");

            // Act
            var sut = new PeekAggregationsFunction(
                mediator.Object,
                identifier,
                new ExternalBundleIdProvider(),
                new ExternalResponseFormatProvider(),
                new ExternalResponseVersionProvider(),
                new MockedMarketOperatorFlowLogHelper());

            var response = await sut.RunAsync(request).ConfigureAwait(false);

            // Assert
            response.Headers.Should()
                .ContainSingle(header =>
                    header.Key.Equals(Constants.BundleIdHeaderName, StringComparison.Ordinal) &&
                    header.Value.Single().Equals(bundleId, StringComparison.Ordinal));
        }

        [Fact]
        public async Task Peek_WithContent_SetsBundleIdHeader()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString("N");

            Uri path = new($"https://localhost?{Constants.BundleIdQueryName}={bundleId}&{Constants.ResponseFormatQueryName}={ResponseFormat}&{Constants.ResponseVersionQueryName}={ResponseVersion}");

            var request = MockHelpers.CreateHttpRequestData(url: path);
            var mediator = new Mock<IMediator>();
            mediator
                .Setup(p => p.Send(It.IsAny<PeekCommand>(), default))
                .ReturnsAsync(new PeekResponse(true, bundleId, Stream.Null, Enumerable.Empty<string>()));
            var identifier = new MockedMarketOperatorIdentity("fake_value");

            // Act
            var sut = new PeekFunction(
                mediator.Object,
                identifier,
                new ExternalBundleIdProvider(),
                new ExternalResponseFormatProvider(),
                new ExternalResponseVersionProvider(),
                new MockedMarketOperatorFlowLogHelper());

            var response = await sut.RunAsync(request).ConfigureAwait(false);

            // Assert
            response.Headers.Should()
                .ContainSingle(header =>
                    header.Key.Equals(Constants.BundleIdHeaderName, StringComparison.Ordinal) &&
                    header.Value.Single().Equals(bundleId, StringComparison.Ordinal));
        }

        [Fact]
        public async Task PeekMasterData_WithContent_SetsBundleIdHeader()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString("N");
            Uri path = new($"https://localhost?{Constants.BundleIdQueryName}={bundleId}&{Constants.ResponseFormatQueryName}={ResponseFormat}&{Constants.ResponseVersionQueryName}={ResponseVersion}");

            var request = MockHelpers.CreateHttpRequestData(url: path);
            var mediator = new Mock<IMediator>();
            mediator
                .Setup(p => p.Send(It.IsAny<PeekMasterDataCommand>(), default))
                .ReturnsAsync(new PeekResponse(true, bundleId, Stream.Null, Enumerable.Empty<string>()));
            var identifier = new MockedMarketOperatorIdentity("fake_value");

            // Act
            var sut = new PeekMasterDataFunction(
                mediator.Object,
                identifier,
                new ExternalBundleIdProvider(),
                new ExternalResponseFormatProvider(),
                new ExternalResponseVersionProvider(),
                new MockedMarketOperatorFlowLogHelper());

            var response = await sut.RunAsync(request).ConfigureAwait(false);

            // Assert
            response.Headers.Should()
                .ContainSingle(header =>
                    header.Key.Equals(Constants.BundleIdHeaderName, StringComparison.Ordinal) &&
                    header.Value.Single().Equals(bundleId, StringComparison.Ordinal));
        }

        [Fact]
        public async Task PeekTimeSeries_BundleIdIsPresentInQuery_SetsBundleIdHeader()
        {
            // Arrange
            var bundleId = Guid.NewGuid().ToString("N");
            Uri path = new($"https://localhost?{Constants.BundleIdQueryName}={bundleId}&{Constants.ResponseFormatQueryName}={ResponseFormat}&{Constants.ResponseVersionQueryName}={ResponseVersion}");

            var request = MockHelpers.CreateHttpRequestData(url: path);
            var mediator = new Mock<IMediator>();
            mediator
                .Setup(p => p.Send(It.IsAny<PeekTimeSeriesCommand>(), default))
                .ReturnsAsync(new PeekResponse(true, bundleId, Stream.Null, Enumerable.Empty<string>()));
            var identifier = new MockedMarketOperatorIdentity("fake_value");

            // Act
            var sut = new PeekTimeSeriesFunction(
                mediator.Object,
                identifier,
                new ExternalBundleIdProvider(),
                new ExternalResponseFormatProvider(),
                new ExternalResponseVersionProvider(),
                new MockedMarketOperatorFlowLogHelper());
            var response = await sut.RunAsync(request).ConfigureAwait(false);

            // Assert
            response.Headers.Should()
                .ContainSingle(header =>
                    header.Key.Equals(Constants.BundleIdHeaderName, StringComparison.Ordinal) &&
                    header.Value.Single().Equals(bundleId, StringComparison.Ordinal));
        }
    }
}
