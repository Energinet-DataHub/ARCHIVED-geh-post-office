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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Domain;
using Energinet.DataHub.PostOffice.Infrastructure.ContentPath;
using Energinet.DataHub.PostOffice.Infrastructure.GetMessage;
using Energinet.DataHub.PostOffice.Tests.Helpers;
using FluentAssertions.Common;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.GetMessage
{
    [UnitTest]
    public class ContentPathStrategyTests
    {
        [Fact]
        public async Task Test_Finding_Correct_Strategy_FromDomain()
        {
            // Arrange
            var dataAvailableStorageService = new Mock<IDataAvailableStorageService>();
            var messageResponseStorage = new Mock<IMessageReplyStorage>();
            messageResponseStorage
                .Setup(e => e.GetMessageReplyAsync(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<string>());

            var strategyFactory = new GetContentPathStrategyFactory(GetContentPathStrategies());
            var dataAvailableController = new DataAvailableController(dataAvailableStorageService.Object, messageResponseStorage.Object, strategyFactory);

            var dataAvailables = TestData.GetRandomValidDataAvailables(5);
            var requestData = new RequestData() { Uuids = dataAvailables.Select(data => data.Uuid!), Origin = "Test" };

            // Act
            var strategy = await dataAvailableController
                .GetStrategyForContentPathAsync(requestData)
                .ConfigureAwait(false);

            // Assert
            strategy.GetType().IsSameOrEqualTo(typeof(ContentPathFromSubDomain));
        }

        [Fact]
        public async Task Test_Finding_Correct_Strategy_FromStorage()
        {
            // Arrange
            var dataAvailables = TestData.GetRandomValidDataAvailables(5).ToList();
            var contentKey = string.Join(";", dataAvailables.Select(e => e.Uuid));

            var dataAvailableStorageService = new Mock<IDataAvailableStorageService>();
            var messageResponseStorage = new Mock<IMessageReplyStorage>();
            messageResponseStorage
                .Setup(e => e.GetMessageReplyAsync(It.IsAny<string>()))
                .ReturnsAsync(contentKey);

            var strategyFactory = new GetContentPathStrategyFactory(GetContentPathStrategies());
            var dataAvailableController = new DataAvailableController(dataAvailableStorageService.Object, messageResponseStorage.Object, strategyFactory);

            var requestData = new RequestData() { Uuids = dataAvailables.Select(data => data.Uuid!), Origin = "Test" };

            // Act
            var strategy = await dataAvailableController
                .GetStrategyForContentPathAsync(requestData)
                .ConfigureAwait(false);

            // Assert
            strategy.GetType().IsSameOrEqualTo(typeof(ContentPathFromSavedResponse));
        }

        private static IEnumerable<IGetContentPathStrategy> GetContentPathStrategies()
        {
            var getPathToDataFromServiceBus = new Mock<IGetPathToDataFromServiceBus>();
            var sendMessageToServiceBus = new Mock<ISendMessageToServiceBus>();

            return new List<IGetContentPathStrategy>() { new ContentPathFromSavedResponse(), new ContentPathFromSubDomain(sendMessageToServiceBus.Object, getPathToDataFromServiceBus.Object) };
        }
    }
}
