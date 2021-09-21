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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.DataAvailable
{
#pragma warning disable CA1001
    public class DataAvailableNotificationSender : IDataAvailableNotificationSender
#pragma warning restore CA1001
    {
        private readonly ServiceBusClient _serviceBusClient;

        public DataAvailableNotificationSender(string connectionString)
        {
            ServiceBusConnectionString = connectionString;
            _serviceBusClient = new ServiceBusClient(ServiceBusConnectionString);
        }

        private string ServiceBusConnectionString { get; init; }
        public async Task SendAsync(DataAvailableNotificationDto dataAvailableNotificationDto)
        {
            if (dataAvailableNotificationDto == null)
                throw new ArgumentNullException(nameof(dataAvailableNotificationDto));

            await using var sender = _serviceBusClient.CreateSender("sbq-dataavailable");
            using var messageBatch = await sender.CreateMessageBatchAsync().ConfigureAwait(false);
            var msg = new Contracts.DataAvailableNotificationContract().ToByteArray();
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(new BinaryData(msg))))
            {
#pragma warning disable CA2201
                throw new Exception("The message is too large to fit in the batch.");
#pragma warning restore CA2201
            }

            Console.WriteLine(
                $"Message added to batch, uuid: {dataAvailableNotificationDto.UUID}, recipient: {dataAvailableNotificationDto.Recipient} ");

            await sender.SendMessagesAsync(messageBatch).ConfigureAwait(false);
        }
    }
}
