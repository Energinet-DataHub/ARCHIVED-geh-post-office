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
using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.SubDomain
{
    public static class MockedMessage
    {
        private static int _sequence;

        public static ServiceBusReceivedMessage Create(byte[] bytes)
        {
            return ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(bytes),
                lockedUntil: DateTimeOffset.UtcNow.AddDays(1),
                sequenceNumber: ++_sequence);
        }
    }
}
