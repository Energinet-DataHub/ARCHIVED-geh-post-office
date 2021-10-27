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

namespace Energinet.DataHub.MessageHub.Core.Factories
{
    public class AzureServiceBusFactory : IMessageBusFactory
    {
        private readonly object _lockObject = new object();

        private readonly ConcurrentDictionary<string, ServiceBusClient> _clients = new();

        private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

        public ISenderMessageBus GetSenderClient(string connectionString, string queueOrTopicName)
        {
            var key = $"{connectionString}-{queueOrTopicName}";

            if (_senders.ContainsKey(key) && !_senders[key].IsClosed)
            {
                return AzureSenderServiceBus.Create(_senders[key]);
            }

            var client = GetServiceBusClient(connectionString);

            lock (_lockObject)
            {
                if (_senders.ContainsKey(key) && _senders[key].IsClosed)
                {
                    if (_senders[key].IsClosed)
                    {
                        if (_senders.TryRemove(key, out var removedSender))
                        {
                            var disposedSender = removedSender.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    return AzureSenderServiceBus.Create(_senders[key]);
                }

                var sender = client.CreateSender(queueOrTopicName);

                _senders[key] = sender;
            }

            return AzureSenderServiceBus.Create(_senders[key]);
        }

        public async Task<AzureSessionReceiverServiceBus> GetSessionReceiverClientAsync(string connectionString, string queueOrTopicName, string sessionId)
        {
            var client = GetServiceBusClient(connectionString);

            var receiver = await client.AcceptSessionAsync(queueOrTopicName, sessionId).ConfigureAwait(false);

            return AzureSessionReceiverServiceBus.Create(receiver);
        }

        protected virtual ServiceBusClient GetServiceBusClient(string connectionString)
        {
            var key = $"{connectionString}";

            lock (_lockObject)
            {
                if (ClientDoesntExistOrIsClosed(connectionString))
                {
                    var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
                    {
                        TransportType = ServiceBusTransportType.AmqpTcp
                    });

                    _clients[key] = client;
                }

                return _clients[key];
            }
        }

        private bool ClientDoesntExistOrIsClosed(string connectionString)
        {
            return !_clients.ContainsKey(connectionString) || _clients[connectionString].IsClosed;
        }
    }
}
