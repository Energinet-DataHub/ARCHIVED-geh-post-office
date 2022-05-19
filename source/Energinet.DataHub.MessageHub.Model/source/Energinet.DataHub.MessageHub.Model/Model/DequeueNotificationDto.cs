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

namespace Energinet.DataHub.MessageHub.Model.Model
{
    /// <summary>
    /// Signals the sub-domains that a market operator has acknowledged the specified data.
    /// Remember to check for LegacyActorIdDto!
    /// <param name="DataAvailableNotificationReferenceId">
    /// A reference id used to obtain the list of dequeue DataAvailableNotification ids.
    /// </param>
    /// <param name="MarketOperator">
    /// The id of the market operator that acknowledged the specified data.
    /// Remember to check for LegacyActorIdDto!
    /// </param>
    /// </summary>
    public sealed record DequeueNotificationDto(string DataAvailableNotificationReferenceId, ActorIdDto MarketOperator);
}
