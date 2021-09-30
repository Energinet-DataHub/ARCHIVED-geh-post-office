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

namespace GreenEnergyHub.PostOffice.Communicator.Model
{
    /// <summary>
    /// Signals the sub-domains that a market operator has acknowledged the specified data.
    /// <param name="DataAvailableNotificationIds">
    /// A collection of guids identifying which data the market operator has approved.
    /// </param>
    /// <param name="GlobalLocationNumberDto">
    /// A Global Location Number identifying a market operator.
    /// </param>
    /// </summary>
    public sealed record DequeueNotificationDto(ICollection<Guid> DataAvailableNotificationIds, GlobalLocationNumberDto GlobalLocationNumberDto);
}
