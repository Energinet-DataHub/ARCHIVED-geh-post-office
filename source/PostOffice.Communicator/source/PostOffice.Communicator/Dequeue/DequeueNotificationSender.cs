﻿// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Model;
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Contracts;
using static System.DateTimeOffset;
using static System.Guid;

namespace Energinet.DataHub.MessageHub.Client.Dequeue
{
    public sealed class DequeueNotificationSender : IDequeueNotificationSender, IAsyncDisposable
    {
        private readonly IServiceBusClientFactory _serviceBusClientFactory;
        private ServiceBusClient? _serviceBusClient;

        public DequeueNotificationSender(IServiceBusClientFactory serviceBusClientFactory)
        {
            _serviceBusClientFactory = serviceBusClientFactory;
        }

        public async Task SendAsync(DequeueNotificationDto dequeueNotificationDto, DomainOrigin domainOrigin)
        {
            if (dequeueNotificationDto is null)
                throw new ArgumentNullException(nameof(dequeueNotificationDto));

            _serviceBusClient ??= _serviceBusClientFactory.Create();
            await using var sender = _serviceBusClient.CreateSender($"sbq-{domainOrigin}-dequeue");

            var contract = new DequeueContract
            {
                DataAvailableNotificationIds = { dequeueNotificationDto.DataAvailableNotificationIds.Select(x => x.ToString()) },
                MarketOperator = dequeueNotificationDto.MarketOperator.Value
            };

            var dequeueMessage = new ServiceBusMessage(new BinaryData(contract.ToByteArray()));
            dequeueMessage.ApplicationProperties.Add("OperationTimestamp", UtcNow);
            dequeueMessage.ApplicationProperties.Add("OperationCorrelationId", NewGuid().ToString());
            dequeueMessage.ApplicationProperties.Add("MessageVersion", 1);
            dequeueMessage.ApplicationProperties.Add("MessageType ", "RequestDataBundleSent");
            dequeueMessage.ApplicationProperties.Add("EventIdentification ", 1);
            await sender.SendMessageAsync(dequeueMessage).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_serviceBusClient != null)
            {
                await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
                _serviceBusClient = null;
            }
        }
    }
}
