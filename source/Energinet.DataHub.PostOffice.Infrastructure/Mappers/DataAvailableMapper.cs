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
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.Commands;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace Energinet.DataHub.PostOffice.Infrastructure.Mappers
{
    public sealed class DataAvailableMapper : IMapper<DataAvailableNotificationDto, DataAvailableNotificationCommand>
    {
        public DataAvailableNotificationCommand Map(DataAvailableNotificationDto obj)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            var dataAvailableCommand = new DataAvailableNotificationCommand(
                obj.Uuid.ToString(),
                obj.Recipient.Value,
                obj.MessageType.Value,
                obj.Origin.ToString(),
                obj.SupportsBundling,
                obj.RelativeWeight);

            return dataAvailableCommand;
        }
    }
}
