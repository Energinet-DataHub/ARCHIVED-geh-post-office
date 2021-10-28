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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Core.Factories;
using Energinet.DataHub.MessageHub.Core.Tests.Peek;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;
using Moq;
using Xunit;

namespace Energinet.DataHub.MessageHub.Core.Tests.Factories
{
    public sealed class AzureServiceBusFactoryTests
    {
        [Fact]
        public void Create_ReturnsServiceBusClientSender_FromExisting()
        {
            // arrange
            var connectionString = "Endpoint=sb://sbn-postoffice.servicebus.windows.net/;SharedAccessKeyName=Hello;SharedAccessKey=there";
            var queueName = "test";

            var messageBusFactory = new AzureServiceBusFactory();
            var target = new ServiceBusClientFactory(connectionString, messageBusFactory);

            // act
            var actualAdd = target.CreateSender(queueName);

            var actualGet = target.CreateSender(queueName);

            // assert
            Assert.NotNull(actualAdd);
            Assert.NotNull(actualGet);
        }

        [Fact]
        public async Task Create_ReturnsServiceBusClientSessionReceiver_FromExisting()
        {
            // arrange
            var connectionString = "Endpoint=sb://sbn-postoffice.servicebus.windows.net/;SharedAccessKeyName=Hello;SharedAccessKey=there";
            var queueName = $"sbq-test";
            var replyQueue = $"sbq-test-reply";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var requestBundleResponse = new DataBundleResponseContract { Success = new DataBundleResponseContract.Types.FileResource { ContentUri = "http://localhost", DataAvailableNotificationIds = { new[] { "A8A6EAA8-DAF3-4E82-910F-A30260CEFDC5" } } } };
            var bytes = requestBundleResponse.ToByteArray();

            var serviceBusReceivedMessage = MockedServiceBusReceivedMessage.Create(bytes);
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();
            serviceBusSessionReceiverMock
                .Setup(x => x.ReceiveMessageAsync(It.IsAny<TimeSpan>(), default))
                .ReturnsAsync(serviceBusReceivedMessage);

            await using var serviceBusClient = new MockedServiceBusClient(
                queueName,
                replyQueue,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var messageBusFactory = new Mock<AzureServiceBusFactory>();
            messageBusFactory
                .Setup(x => x.GetServiceBusClient(connectionString))
                .Returns(serviceBusClient);

            var target = new ServiceBusClientFactory(connectionString, messageBusFactory.Object);

            // act
            var actualAdd = await target.CreateSessionReceiverAsync(replyQueue, It.IsAny<string>()).ConfigureAwait(false);

            // assert
            Assert.NotNull(actualAdd);
        }
    }
}
