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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;

namespace Energinet.DataHub.PostOffice.Domain.Services
{
    /// <summary>
    /// Service logging steps in a market operator flow
    /// </summary>
    public interface IMarketOperatorFlowLogger
    {
        /// <summary>
        /// Logs message regarding registry actor found
        /// </summary>
        Task LogActorFoundAsync(Guid externalActorId, Guid actorId);

        /// <summary>
        /// Logs message regarding registry actor not found
        /// </summary>
        Task LogActorNotFoundAsync(Guid externalActorId);

        /// <summary>
        /// Logs message regarding legacy actor found
        /// </summary>
        Task LogLegacyActorFoundAsync(Guid externalActorId, string gln);

        /// <summary>
        /// Logs message regarding legacy actor not found
        /// </summary>
        Task LogLegacyActorNotFoundAsync(Guid externalActorId);

        /// <summary>
        /// Logs which sub domain is called for data retrieving bundle data
        /// </summary>
        Task LogSubDomainOriginDataRequestAsync(DomainOrigin origin);

        /// <summary>
        /// Retrieves all logged messages as a string
        /// </summary>
        /// <returns>The log</returns>
        Task<string> GetLogAsync();
    }
}