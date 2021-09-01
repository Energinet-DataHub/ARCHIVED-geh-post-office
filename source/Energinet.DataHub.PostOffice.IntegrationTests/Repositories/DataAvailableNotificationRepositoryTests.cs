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
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Repositories
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public sealed class DataAvailableNotificationRepositoryTests
    {
        [Fact]
        public async Task SaveAsync_NullData_ThrowsArgumentNullException()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var target = scope.GetInstance<IDataAvailableNotificationRepository>();

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.SaveAsync(null!)).ConfigureAwait(false);
        }

        [Fact]
        public async Task SaveAndGet_WithData_GetCorrectData()
        {
            // Arrange
            await using var host = await MarketOperatorIntegrationTestHost
                .InitializeAsync()
                .ConfigureAwait(false);

            await using var scope = host.BeginScope();
            var target = scope.GetInstance<IDataAvailableNotificationRepository>();

            var model = new DataAvailableNotification(
                new Uuid(Guid.NewGuid()),
                new MarketOperator(new GlobalLocationNumber("fake_value")),
                ContentType.TimeSeries,
                DomainOrigin.TimeSeries,
                new Weight(5));

            // Act
            await target.SaveAsync(model).ConfigureAwait(false);

            // Assert
        }
    }
}
