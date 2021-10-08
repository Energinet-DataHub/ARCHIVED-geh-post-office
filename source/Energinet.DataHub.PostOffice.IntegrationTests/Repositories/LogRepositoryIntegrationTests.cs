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

using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Model.Logging;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories
{
    [UnitTest]
    public class LogRepositoryIntegrationTests
    {
        [Fact]
        public async Task SaveLogOccurrenceAsync_ValidData_LogOccurrenceIsSavedToStorage()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            var container = scope.GetInstance<ILogRepositoryContainer>();

            var target = new LogRepository(container);

            var fakeIBundleContent = new Mock<IBundleContent>();
            fakeIBundleContent
                .Setup(e => e.LogIdentifier).Returns("https://127.0.0.1");

            var logObject = new Log(
                "fake_value",
                new GlobalLocationNumber("fake_value"),
                "fake_value",
                "fake_value",
                new Reply(fakeIBundleContent.Object),
                "fake_value");

            // Act
            var actual = await target.SaveLogOccurrenceAsync(logObject).ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.NotEmpty(actual);
        }
    }
}
