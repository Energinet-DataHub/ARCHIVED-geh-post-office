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
using System.Globalization;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Model.IntegrationEvents;
using NodaTime;

namespace Energinet.DataHub.MessageHub.Core.Extensions
{
    internal static class ServiceBusMessageExtensions
    {
        public static ServiceBusMessage AddDequeueIntegrationEvents(this ServiceBusMessage serviceBusMessage, string operationCorrelationId)
        {
            return serviceBusMessage.AddIntegrationsEvents(
                operationCorrelationId,
                IntegrationEventsMessageType.Dequeue,
                Guid.NewGuid().ToString());
        }

        public static ServiceBusMessage AddRequestDataBundleIntegrationEvents(this ServiceBusMessage serviceBusMessage, string operationCorrelationId)
        {
            return serviceBusMessage.AddIntegrationsEvents(
                operationCorrelationId,
                IntegrationEventsMessageType.RequestDataBundle,
                Guid.NewGuid().ToString());
        }

        private static ServiceBusMessage AddIntegrationsEvents(
            this ServiceBusMessage serviceBusMessage,
            string operationCorrelationId,
            IntegrationEventsMessageType messageType,
            string eventIdentification)
        {
            ArgumentNullException.ThrowIfNull(serviceBusMessage, nameof(serviceBusMessage));

            serviceBusMessage.ApplicationProperties.Add("OperationTimestamp", SystemClock.Instance.GetCurrentInstant().ToString(null, CultureInfo.InvariantCulture));
            serviceBusMessage.ApplicationProperties.Add("OperationCorrelationId", operationCorrelationId);
            serviceBusMessage.ApplicationProperties.Add("MessageVersion", 1);
            serviceBusMessage.ApplicationProperties.Add("MessageType", messageType.ToString());
            serviceBusMessage.ApplicationProperties.Add("EventIdentification", eventIdentification);
            return serviceBusMessage;
        }
    }
}
