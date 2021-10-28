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

using Energinet.DataHub.MessageHub.Core.Factories;
using Moq;
using Xunit;

namespace Energinet.DataHub.MessageHub.Core.Tests.Factories
{
    public sealed class ServiceBusClientFactoryTests
    {
        [Fact]
        public void Create_ReturnsServiceBusClientSender()
        {
            // arrange
            var connectionString = "Endpoint=sb://sbn-postoffice.servicebus.windows.net/;SharedAccessKeyName=Hello;SharedAccessKey=there";
            var queueName = "test";

            var messageBusFactory = new AzureServiceBusFactory();
            var target = new ServiceBusClientFactory(connectionString, messageBusFactory);

            // act
            var actual = target.CreateSender(queueName);

            // assert
            Assert.NotNull(actual);
        }

        [Fact]
        public void Create_ReturnsServiceBusClientSessionReceiver()
        {
            // arrange
            var connectionString = "Endpoint=sb://sbn-postoffice.servicebus.windows.net/;SharedAccessKeyName=Hello;SharedAccessKey=there";
            var queueName = "test";
            var sessionId = It.IsAny<string>();

            var messageBusFactory = new AzureServiceBusFactory();
            var target = new ServiceBusClientFactory(connectionString, messageBusFactory);

            // act
            var actual = target.CreateSessionReceiverAsync(queueName, sessionId);

            // assert
            Assert.NotNull(actual);
        }
    }
}
