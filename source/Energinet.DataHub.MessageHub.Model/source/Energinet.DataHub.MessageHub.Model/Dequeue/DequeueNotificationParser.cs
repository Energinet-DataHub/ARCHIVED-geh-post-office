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
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;

namespace Energinet.DataHub.MessageHub.Model.Dequeue
{
    public sealed class DequeueNotificationParser : IDequeueNotificationParser
    {
        public DequeueNotificationDto Parse(byte[] dequeueNotificationContract)
        {
            try
            {
                var dequeueContract = DequeueContract.Parser.ParseFrom(dequeueNotificationContract);

                var marketOperator = Guid.TryParse(dequeueContract.MarketOperator, out var actorId)
                    ? new ActorIdDto(actorId)
#pragma warning disable CS0618 // Type or member is obsolete
                    : new LegacyActorIdDto(dequeueContract.MarketOperator);
#pragma warning restore CS0618 // Type or member is obsolete

                return new DequeueNotificationDto(
                    dequeueContract.DataAvailableNotificationReferenceId,
                    marketOperator);
            }
            catch (Exception ex) when (ex is InvalidProtocolBufferException or FormatException)
            {
                throw new MessageHubException("Error parsing bytes for DequeueNotificationDto.", ex);
            }
        }

        public byte[] Parse(DequeueNotificationDto dequeueNotificationDto)
        {
            ArgumentNullException.ThrowIfNull(dequeueNotificationDto, nameof(dequeueNotificationDto));

#pragma warning disable CS0618 // Type or member is obsolete
            var marketOperator = dequeueNotificationDto.MarketOperator is LegacyActorIdDto legacyActorIdDto
#pragma warning restore CS0618 // Type or member is obsolete
                ? legacyActorIdDto.LegacyValue
                : dequeueNotificationDto.MarketOperator.Value.ToString();

            var message = new DequeueContract
            {
                DataAvailableNotificationReferenceId = dequeueNotificationDto.DataAvailableNotificationReferenceId,
                MarketOperator = marketOperator
            };

            return message.ToByteArray();
        }
    }
}
