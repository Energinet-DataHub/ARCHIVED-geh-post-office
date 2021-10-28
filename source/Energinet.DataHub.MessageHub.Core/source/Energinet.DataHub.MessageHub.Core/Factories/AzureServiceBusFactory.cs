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

using System.Collections.Concurrent;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Model.Exceptions;

namespace Energinet.DataHub.MessageHub.Core.Factories
{
    public class AzureServiceBusFactory : IMessageBusFactory
    {
        private readonly ConcurrentDictionary<string, ServiceBusClient> _clients = new();

        private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

        public ISenderMessageBus GetSenderClient(string connectionString, string queueOrTopicName)
        {
            var key = $"{connectionString}-{queueOrTopicName}";

            if (_senders.ContainsKey(key))
            {
                if (_senders.TryGetValue(key, out var sender))
                    return AzureSenderServiceBus.Create(sender);

                throw new MessageHubException("sender not found");
            }
            else
            {
                var client = GetServiceBusClient(connectionString);

                if (_senders.TryAdd(key, client.CreateSender(queueOrTopicName))
                    && _senders.TryGetValue(key, out var sender))
                    return AzureSenderServiceBus.Create(sender);

                throw new MessageHubException("sender not found");
            }
        }

        public async Task<AzureSessionReceiverServiceBus> GetSessionReceiverClientAsync(string connectionString, string queueOrTopicName, string sessionId)
        {
            var client = GetServiceBusClient(connectionString);

            var receiver = await client.AcceptSessionAsync(queueOrTopicName, sessionId).ConfigureAwait(false);

            return AzureSessionReceiverServiceBus.Create(receiver);
        }

        public virtual ServiceBusClient GetServiceBusClient(string connectionString)
        {
            var key = $"{connectionString}";

            if (ClientDoesntExistOrIsClosed(connectionString))
            {
                var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpTcp
                });

                _clients.TryAdd(key, client);
            }

            _clients.TryGetValue(key, out var dictServiceBusClient);

            return dictServiceBusClient ?? throw new MessageHubException("ServiceBusClient not found in dictionary");
        }

        private bool ClientDoesntExistOrIsClosed(string connectionString)
        {
            return !_clients.TryGetValue(connectionString, out var client) || client.IsClosed;
        }
    }
}
