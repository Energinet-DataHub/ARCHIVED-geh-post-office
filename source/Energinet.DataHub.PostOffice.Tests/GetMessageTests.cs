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
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.GetMessage.Handlers;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Application.GetMessage.Queries;
using Energinet.DataHub.PostOffice.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.PostOffice.Tests
{
    public class GetMessageTests
    {
        public GetMessageTests()
        {
        }

        [Fact]
        public void GetMessageHandler_CallFromMarketOperator_ResultMustMatch()
        {
            var dataAvailableStorageService = new Mock<IDataAvailableStorageService>();
            dataAvailableStorageService.Setup(
                service => service.GetDataAvailableUuidsAsync(
                    It.IsAny<GetMessageQuery>())).ReturnsAsync(It.IsAny<List<string>>());

            var sendMessageToServiceBusEntityMock = new Mock<ISendMessageToServiceBus>();
            SendMessageToServiceBus(sendMessageToServiceBusEntityMock);

            var readMessageFromServiceBusEntityMock = new Mock<IGetPathToDataFromServiceBus>();
            ReadMessageFromServiceBusEntity(readMessageFromServiceBusEntityMock);

            var storageServiceMock = new Mock<IStorageService>();
            GetMarketOperatorDataFromStorageService(storageServiceMock);

            GetMessageQuery query = new GetMessageQuery(It.IsAny<string>());
            GetMessageHandler handler = new GetMessageHandler(
                dataAvailableStorageService.Object,
                sendMessageToServiceBusEntityMock.Object,
                readMessageFromServiceBusEntityMock.Object,
                storageServiceMock.Object);

            // Act
            var result = handler.Handle(query, System.Threading.CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.GetAwaiter().GetResult().Should().Be(GetStorageContentAsyncSimulatedData());
        }

        private static void GetMarketOperatorDataFromStorageService(Mock<IStorageService> storageService)
        {
            var data = storageService.Setup(
                ss => ss.GetStorageContentAsync(
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetStorageContentAsyncSimulatedData());
        }

        private static void ReadMessageFromServiceBusEntity(Mock<IGetPathToDataFromServiceBus> readMessageFromServiceBusEntityMock)
        {
            readMessageFromServiceBusEntityMock.Setup(
                    getPathToDataFromServiceBus => getPathToDataFromServiceBus.GetPathAsync(
                        It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<string>());
        }

        private static void SendMessageToServiceBus(Mock<ISendMessageToServiceBus> sendMessageToServiceBusEntityMock)
        {
            sendMessageToServiceBusEntityMock.Setup(sendMessageToServiceBus =>
                sendMessageToServiceBus.SendMessageAsync(
                    It.IsNotNull<List<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()));
        }

        private static void GetDocumentsAsync(Mock<IDocumentStore<DataAvailable>> storageServiceMock)
        {
            var result = storageServiceMock.Setup(
                storageService => storageService.GetDocumentsAsync(
                    It.IsAny<GetMessageQuery>())).ReturnsAsync(CreateListOfDataAvailableObjects());
        }

        private static string GetStorageContentAsyncSimulatedData()
        {
            return "test data";
        }

        private static IList<DataAvailable> CreateListOfDataAvailableObjects()
        {
            return new List<DataAvailable>()
            {
                new DataAvailable(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>(),
                    It.IsAny<decimal>()),
                new DataAvailable(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>(),
                    It.IsAny<decimal>()),
            };
        }
    }
}
