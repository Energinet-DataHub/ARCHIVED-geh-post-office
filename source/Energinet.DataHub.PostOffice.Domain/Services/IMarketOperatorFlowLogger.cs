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
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Domain.Model;
using DomainOrigin = Energinet.DataHub.PostOffice.Domain.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Domain.Services
{
    /// <summary>
    /// Service logging steps in a market operator flow
    /// </summary>
    public interface IMarketOperatorFlowLogger
    {
        /// <summary>
        /// Enables heavy logging.
        /// </summary>
        bool EnableHeavyLogging { get; set; }

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
        /// Logs message regarding a timeout when requesting data from a subdomain.
        /// </summary>
        Task LogRequestDataFromSubdomainTimeoutAsync(string correlationId, DomainOrigin origin);

        /// <summary>
        /// Logs message regarding an error when requesting data from a subdomain.
        /// </summary>
        Task LogRequestErrorFromSubdomainAsync(string correlationId, DomainOrigin origin, DataBundleResponseErrorDto? errorMessage);

        /// <summary>
        /// Logs information about the found bundle.
        /// </summary>
        Task LogFoundBundleAsync(string bundleDocumentRecipient, string bundleDocumentId);

        /// <summary>
        /// Logs which recipient is used to search for a bundle.
        /// </summary>
        Task LogSearchForExistingBundleAsync(ActorId recipient);

        /// <summary>
        /// Logs which sub domain is called for data retrieving bundle data
        /// </summary>
        Task LogSubDomainOriginDataRequestAsync(DomainOrigin origin);

        /// <summary>
        /// Logs that the specified domain has a catalog entry for the next notification.
        /// </summary>
        Task LogCatalogWasFoundForDomainAsync(ActorId recipient, DomainOrigin domain);

        /// <summary>
        /// Logs that the specified domain does not have a catalog entry for the next notification.
        /// </summary>
        Task LogNoCatalogWasFoundForDomainAsync(ActorId recipient, DomainOrigin domain);

        /// <summary>
        /// Log messages regarding DataAvailable notifications for specified domains.
        /// </summary>
        Task LogLatestDataAvailableNotificationsAsync(ActorId marketOperator, DomainOrigin[] domains);

        /// <summary>
        /// Logs which actor requested a dequeue
        /// </summary>
        /// <param name="actorId">The id of the actor trying to dequeue</param>
        /// <param name="correlationId">The correlation id of the dequeue call</param>
        /// <param name="bundleId">the bundle id they are trying to dequeue</param>
        Task LogActorDequeueingAsync(ActorId actorId, string correlationId, string bundleId);

        /// <summary>
        /// Retrieves all logged messages as a string
        /// </summary>
        /// <returns>The log</returns>
        Task<string> GetLogAsync();

        /// <summary>
        /// Logs that no notifications were found.
        /// </summary>
        Task LogNoNotificationsFoundAsync();

        /// <summary>
        /// Logs that no response was received from domain.
        /// </summary>
        Task LogNoResponseAsync();
    }
}
