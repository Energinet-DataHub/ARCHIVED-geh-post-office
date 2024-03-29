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
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions
{
    public class DataAvailableMessageReceiver : IDataAvailableMessageReceiver
    {
        private readonly ServiceBusReceiver _messageReceiver;
        private readonly int _batchSize;
        private readonly TimeSpan _timeout;

        public DataAvailableMessageReceiver(ServiceBusReceiver messageReceiver, int batchSize, TimeSpan timeout)
        {
            _messageReceiver = messageReceiver;
            _batchSize = batchSize;
            _timeout = timeout;
        }

        public async Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveAsync()
        {
            var messages = await _messageReceiver.ReceiveMessagesAsync(_batchSize, _timeout).ConfigureAwait(false);
            return messages != null
                ? messages.ToList()
                : Array.Empty<ServiceBusReceivedMessage>();
        }

        public Task DeadLetterAsync(IEnumerable<ServiceBusReceivedMessage> messages)
        {
            var tasks = messages
                .Where(x => x.LockedUntil > DateTime.UtcNow)
                .Select(x => _messageReceiver.DeadLetterMessageAsync(x));
            return Task.WhenAll(tasks);
        }

        public Task CompleteAsync(IEnumerable<ServiceBusReceivedMessage> messages)
        {
            var lockTokens = messages
                .Where(x => x.LockedUntil > DateTime.UtcNow)
                .Select(x => _messageReceiver.CompleteMessageAsync(x));
            return Task.WhenAll(lockTokens);
        }
    }
}
