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
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Services.Infrastructure
{
    [UnitTest]
    public sealed class MarketOperatorFlowLoggerTests
    {
        [Fact]
        public async Task LogActorFoundAsync_IdsSupplied_Logs()
        {
            // assert
            var externalActorId = Guid.NewGuid();
            var actorId = Guid.NewGuid();
            var target = new MarketOperatorFlowLogger(
                new Mock<IDataAvailableNotificationRepository>().Object,
                new Mock<ILogger>().Object);

            // act
            await target.LogActorFoundAsync(externalActorId, actorId);
            var actual = await target.GetLogAsync();

            // assert
            Assert.StartsWith("Actor found", actual, StringComparison.CurrentCulture);
            Assert.Contains(externalActorId.ToString(), actual, StringComparison.CurrentCultureIgnoreCase);
            Assert.Contains(actorId.ToString(), actual, StringComparison.CurrentCultureIgnoreCase);
        }

        [Fact]
        public async Task LogActorNotFoundAsync_IdsSupplied_Logs()
        {
            // assert
            var externalActorId = Guid.NewGuid();
            var target = new MarketOperatorFlowLogger(
                new Mock<IDataAvailableNotificationRepository>().Object,
                new Mock<ILogger>().Object);

            // act
            await target.LogActorNotFoundAsync(externalActorId);
            var actual = await target.GetLogAsync();

            // assert
            Assert.StartsWith("An actor was not found", actual, StringComparison.CurrentCulture);
            Assert.Contains(externalActorId.ToString(), actual, StringComparison.CurrentCultureIgnoreCase);
        }

        [Fact]
        public async Task LogLegacyActorFoundAsync_IdsSupplied_Logs()
        {
            // assert
            var externalActorId = Guid.NewGuid();
            var gln = Guid.NewGuid().ToString();
            var target = new MarketOperatorFlowLogger(
                new Mock<IDataAvailableNotificationRepository>().Object,
                new Mock<ILogger>().Object);

            // act
            await target.LogLegacyActorFoundAsync(externalActorId, gln);
            var actual = await target.GetLogAsync();

            // assert
            Assert.StartsWith("Legacy actor found", actual, StringComparison.CurrentCulture);
            Assert.Contains(externalActorId.ToString(), actual, StringComparison.CurrentCultureIgnoreCase);
            Assert.Contains(gln, actual, StringComparison.CurrentCultureIgnoreCase);
        }

        [Fact]
        public async Task LogLegacyActorNotFoundAsync_IdsSupplied_Logs()
        {
            // assert
            var externalActorId = Guid.NewGuid();
            var target = new MarketOperatorFlowLogger(
                new Mock<IDataAvailableNotificationRepository>().Object,
                new Mock<ILogger>().Object);

            // act
            await target.LogLegacyActorNotFoundAsync(externalActorId);
            var actual = await target.GetLogAsync();

            // assert
            Assert.StartsWith("A legacy actor was not found", actual, StringComparison.CurrentCulture);
            Assert.Contains(externalActorId.ToString(), actual, StringComparison.CurrentCultureIgnoreCase);
        }

        [Fact]
        public async Task GetLog_ReturnsEntriesCorrectlyOrdered()
        {
            // assert
            var externalActorIdFirst = Guid.NewGuid();
            var externalActorIdSecond = Guid.NewGuid();
            var target = new MarketOperatorFlowLogger(
                new Mock<IDataAvailableNotificationRepository>().Object,
                new Mock<ILogger>().Object);

            await target.LogActorNotFoundAsync(externalActorIdFirst);
            await target.LogLegacyActorNotFoundAsync(externalActorIdSecond);

            // act
            var actual = (await target.GetLogAsync()).Split(Environment.NewLine).ToList();

            // assert
            Assert.Contains(externalActorIdFirst.ToString(), actual[0], StringComparison.CurrentCultureIgnoreCase);
            Assert.Contains(externalActorIdSecond.ToString(), actual[1], StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
