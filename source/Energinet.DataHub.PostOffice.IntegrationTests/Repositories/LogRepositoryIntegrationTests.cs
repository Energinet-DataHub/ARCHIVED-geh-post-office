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
using Energinet.DataHub.PostOffice.Domain.Model.Logging;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Microsoft.Azure.Cosmos;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public class LogRepositoryIntegrationTests
    {
        [Fact]
        public async Task SaveLogOccurrenceAsync_PeekLogValidData_LogOccurrenceIsSavedToStorage()
        {
            // Arrange
            var container = await SetUpTestBasis().ConfigureAwait(true);

            var target = new LogRepository(container);

            var fakeIBundleContent = new Mock<IBundleContent>();
            fakeIBundleContent
                .Setup(e => e.LogIdentifier).Returns("https://127.0.0.1");

            var processId = GetFakeProcessId();

            var logObject = new PeekLog(
                processId,
                fakeIBundleContent.Object);

            // Act
            await target.SavePeekLogOccurrenceAsync(logObject).ConfigureAwait(true);

            var cosmosItem = await container.Container.ReadItemAsync<CosmosLog>(
                logObject.Id.ToString(),
                new PartitionKey(logObject.ProcessId.Recipient.Gln.Value))
                .ConfigureAwait(true);

            // Assert
            Assert.Equal(logObject.Id.ToString(), cosmosItem.Resource.Id);
        }

        [Fact]
        public async Task SaveLogOccurrenceAsync_DequeueLogValidData_LogOccurrenceIsSavedToStorage()
        {
            // Arrange
            var container = await SetUpTestBasis().ConfigureAwait(false);

            var target = new LogRepository(container);

            var processId = GetFakeProcessId();

            var logObject = new DequeueLog(processId);

            // Act
            await target.SaveDequeueLogOccurrenceAsync(logObject).ConfigureAwait(false);

            // Assert
        }

        private static async Task<ILogRepositoryContainer> SetUpTestBasis()
        {
            await using var host = await MarketOperatorIntegrationTestHost.InitializeAsync().ConfigureAwait(false);
            var scope = host.BeginScope();

            return scope.GetInstance<ILogRepositoryContainer>();
        }

        private static ProcessId GetFakeProcessId()
        {
            return new(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")));
        }
    }
}
