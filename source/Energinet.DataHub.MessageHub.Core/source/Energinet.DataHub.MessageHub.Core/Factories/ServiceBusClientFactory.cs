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

using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.MessageHub.Core.Factories
{
    public sealed class ServiceBusClientFactory : IServiceBusClientFactory
    {
        private readonly object _lockObject = new();
        private readonly string _connectionString;
        private ServiceBusClient? _serviceBusClient;

        public ServiceBusClientFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ServiceBusClient Create()
        {
            if (_serviceBusClient != null)
                return _serviceBusClient;

            lock (_lockObject)
            {
                _serviceBusClient ??= new(_connectionString);
            }

            return _serviceBusClient;
        }
    }
}
