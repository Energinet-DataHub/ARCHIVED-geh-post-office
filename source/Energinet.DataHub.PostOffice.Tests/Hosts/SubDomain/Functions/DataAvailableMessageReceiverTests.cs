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
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.SubDomain.Functions
{
    [UnitTest]
    public class DataAvailableMessageReceiverTests
    {
        [Fact]
        public async Task ReceiveAsync_DelegatesWithCorrectBatchSizeAndTimeout()
        {
            // arrange
            const int batchSize = 11;
            var timeout = TimeSpan.FromSeconds(10);
            var expectedMessages = new List<ServiceBusReceivedMessage>();

            var messageReceiverMock = new Mock<ServiceBusReceiver>();
            messageReceiverMock.Setup(x => x.ReceiveMessagesAsync(batchSize, timeout, default)).Returns(Task.FromResult<IReadOnlyList<ServiceBusReceivedMessage>>(expectedMessages));

            var target = new DataAvailableMessageReceiver(messageReceiverMock.Object, batchSize, timeout);

            // act
            var actual = await target.ReceiveAsync().ConfigureAwait(false);

            // assert
            Assert.Equal(expectedMessages, actual);
        }

        [Fact]
        public async Task ReceiveAsync_InternalReceiverReturnsNull_ReturnsEmptyList()
        {
            // arrange
            var messageReceiverMock = new Mock<ServiceBusReceiver>();
            messageReceiverMock.Setup(x => x.ReceiveMessagesAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), default)).Returns(Task.FromResult<IReadOnlyList<ServiceBusReceivedMessage>>(null!));

            var target = new DataAvailableMessageReceiver(messageReceiverMock.Object, 1, TimeSpan.Zero);

            // act
            var actual = await target.ReceiveAsync().ConfigureAwait(false);

            // assert
            Assert.NotNull(actual);
            Assert.Empty(actual);
        }

        [Fact]
        public async Task DeadLetterAsync_DelegatesToInternalDeadLetterAsync()
        {
            // arrange
            var message = MockedMessage.Create(Array.Empty<byte>(), Guid.NewGuid());

            var messageReceiverMock = new Mock<ServiceBusReceiver>();
            messageReceiverMock.Setup(x =>
                    x.ReceiveMessagesAsync(
                        It.IsAny<int>(),
                        It.IsAny<TimeSpan>(),
                        default))
                .Returns(
                    Task.FromResult<IReadOnlyList<ServiceBusReceivedMessage>>(new List<ServiceBusReceivedMessage> { message }));

            var target = new DataAvailableMessageReceiver(messageReceiverMock.Object, 1, TimeSpan.Zero);

            // act
            await target.DeadLetterAsync(new[] { message }).ConfigureAwait(false);

            // assert
            messageReceiverMock.Verify(x => x.DeadLetterMessageAsync(message, null, default));
        }

        [Fact]
        public async Task CompleteAsync_DelegatesToInternalCompleteAsync()
        {
            // arrange
            var messages = new List<ServiceBusReceivedMessage> { MockedMessage.Create(Array.Empty<byte>(), Guid.NewGuid()) };

            var messageReceiverMock = new Mock<ServiceBusReceiver>();
            messageReceiverMock.Setup(x =>
                    x.ReceiveMessagesAsync(
                        It.IsAny<int>(),
                        It.IsAny<TimeSpan>(),
                        default))
                .Returns(
                    Task.FromResult<IReadOnlyList<ServiceBusReceivedMessage>>(messages));

            var target = new DataAvailableMessageReceiver(messageReceiverMock.Object, 1, TimeSpan.Zero);

            // act
            await target.CompleteAsync(messages).ConfigureAwait(false);

            // assert
            messageReceiverMock.Verify(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), default));
        }
    }
}
