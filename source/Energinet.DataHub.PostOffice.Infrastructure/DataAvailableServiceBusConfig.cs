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

namespace Energinet.DataHub.PostOffice.Infrastructure
{
    public sealed class DataAvailableServiceBusConfig
    {
        public const string DataAvailableQueueNameKey = "DATAAVAILABLE_QUEUE_NAME";
        public const string DataAvailableQueueConnectionStringKey = "DATAAVAILABLE_QUEUE_CONNECTION_STRING";

        public DataAvailableServiceBusConfig(
            string dataAvailableQueueName,
            string dataAvailableQueueConnectionString)
        {
            if (string.IsNullOrWhiteSpace(dataAvailableQueueName))
                throw new InvalidOperationException($"{DataAvailableQueueNameKey} must be specified in {nameof(DataAvailableServiceBusConfig)}");

            if (string.IsNullOrWhiteSpace(dataAvailableQueueConnectionString))
                throw new InvalidOperationException($"{DataAvailableQueueConnectionStringKey} must be specified in {nameof(DataAvailableServiceBusConfig)}");

            DataAvailableQueueName = dataAvailableQueueName;
            DataAvailableQueueConnectionString = dataAvailableQueueConnectionString;
        }

        public string DataAvailableQueueName { get; }
        public string DataAvailableQueueConnectionString { get; }
    }
}
