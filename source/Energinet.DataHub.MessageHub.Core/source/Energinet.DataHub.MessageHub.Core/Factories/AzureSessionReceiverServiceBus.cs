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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.MessageHub.Core.Factories
{
    public sealed class AzureSessionReceiverServiceBus : IReceiverMessageBus, IAsyncDisposable
    {
        private ServiceBusSessionReceiver? _serviceBusSessionReceiver;

        internal AzureSessionReceiverServiceBus(ServiceBusSessionReceiver serviceBusSessionReceiver)
        {
            _serviceBusSessionReceiver = serviceBusSessionReceiver;
        }

        public async ValueTask DisposeAsync()
        {
            if (_serviceBusSessionReceiver != null)
            {
                await _serviceBusSessionReceiver.DisposeAsync().ConfigureAwait(false);
                _serviceBusSessionReceiver = null;
            }
        }

        public async Task<ServiceBusReceivedMessage?> ReceiveMessageAsync<T>(TimeSpan timeout)
        {
            if (_serviceBusSessionReceiver != null)
                return await _serviceBusSessionReceiver.ReceiveMessageAsync(timeout).ConfigureAwait(false);
            return null;
        }

        internal static AzureSessionReceiverServiceBus Create(ServiceBusSessionReceiver sessionReceiver)
        {
            return new(sessionReceiver);
        }
    }
}
