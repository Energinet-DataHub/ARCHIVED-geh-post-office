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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using DomainOrigin = Energinet.DataHub.PostOffice.Domain.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services
{
    public sealed class MarketOperatorFlowLogger : IMarketOperatorFlowLogger
    {
        private readonly ConcurrentQueue<string> _log = new();

        private readonly FindLatestDataAvailableNotificationRepository _dataAvailableNotificationRepository;
        private readonly ILogger _logger;

        public MarketOperatorFlowLogger(
            FindLatestDataAvailableNotificationRepository dataAvailableNotificationRepository,
            ILogger logger)
        {
            _dataAvailableNotificationRepository = dataAvailableNotificationRepository;
            _logger = logger;
        }

        public bool EnableHeavyLogging { get; set; }

        public Task LogActorFoundAsync(Guid externalActorId, Guid actorId)
        {
            return LogAsync($"Does actor '{externalActorId}' exist in new registry: Yes with id '{actorId}'.");
        }

        public Task LogActorNotFoundAsync(Guid externalActorId)
        {
            return LogAsync($"Does actor '{externalActorId}' exist in new registry: No.");
        }

        public Task LogLegacyActorFoundAsync(Guid externalActorId, string gln)
        {
            return LogAsync($"Does actor '{externalActorId}' exist in legacy registry: Yes with id '{gln}'.");
        }

        public Task LogLegacyActorNotFoundAsync(Guid externalActorId)
        {
            return LogAsync($"Does actor '{externalActorId}' exist in legacy registry: No.");
        }

        public Task LogNoCatalogWasFoundForDomainAsync(ActorId recipient, DomainOrigin domain)
        {
            return LogAsync($"Checking local store for notifications from '{domain}' for actor '{recipient}': Nothing was found.");
        }

        public Task LogCatalogWasFoundForDomainAsync(ActorId recipient, DomainOrigin domain)
        {
            return LogAsync($"Checking local store for notifications from '{domain}' for actor '{recipient}': Items were found.");
        }

        public async Task LogLatestDataAvailableNotificationsAsync(ActorId marketOperator, DomainOrigin[] domains)
        {
            if (!EnableHeavyLogging)
                return;

            ArgumentNullException.ThrowIfNull(marketOperator);
            ArgumentNullException.ThrowIfNull(domains);

            foreach (var domainOrigin in domains)
            {
                var (notification, timestamp, isDequeued) = await _dataAvailableNotificationRepository
                    .FindLatestDataAvailableNotificationAsync(marketOperator, domainOrigin)
                    .ConfigureAwait(false);

                if (notification != null)
                {
                    if (isDequeued)
                    {
                        await LogAsync($"Showing latest DataAvailable for actor '{marketOperator}' from domain '{domainOrigin}': {notification.NotificationId} with {timestamp:u}.").ConfigureAwait(false);
                    }
                    else
                    {
                        await LogAsync($"! Showing latest DataAvailable for actor '{marketOperator}' from domain '{domainOrigin}': {notification.NotificationId} with {timestamp:u}.").ConfigureAwait(false);
                    }
                }
                else
                {
                    await LogAsync($"Showing latest DataAvailable for actor '{marketOperator}' from domain '{domainOrigin}': Nothing was found.").ConfigureAwait(false);
                }
            }
        }

        public Task LogSearchForExistingBundleAsync(ActorId recipient)
        {
            return LogAsync($"Searching for any existing bundle for actor '{recipient}'.");
        }

        public Task LogFoundBundleAsync(string bundleDocumentRecipient, string bundleDocumentId)
        {
            return LogAsync($"Existing bundle '{bundleDocumentId}' found for actor '{bundleDocumentRecipient}'.");
        }

        public Task LogSubDomainOriginDataRequestAsync(DomainOrigin origin)
        {
            return LogAsync($"Bundle exists, must generate message. Asking domain '{origin}' for data.");
        }

        public Task LogActorDequeueingAsync(ActorId actorId, string correlationId, string bundleId)
        {
            return LogAsync($"Actor with id '{actorId}' is trying to dequeue the following bundle '{bundleId}', with correlation id '{correlationId}'.");
        }

        public Task LogRequestDataFromSubdomainTimeoutAsync(string correlationId, DomainOrigin origin)
        {
            return LogAsync($"The request sent to '{origin}' encountered a timeout (30 seconds) while waiting for response, correlationId '{correlationId}'.");
        }

        public async Task LogRequestErrorFromSubdomainAsync(
            string correlationId,
            DomainOrigin origin,
            DataBundleResponseErrorDto? errorMessage)
        {
            await LogAsync($"The request sent to '{origin}' encountered an error, correlationId '{correlationId}'.")
                .ConfigureAwait(false);

            if (errorMessage != null)
            {
                await LogAsync($"The domain error was {errorMessage.Reason}. Additional description: {errorMessage.FailureDescription}.")
                    .ConfigureAwait(false);
            }
            else
            {
                await LogAsync($"The responding domain did not provide an error description.")
                    .ConfigureAwait(false);
            }
        }

        public Task<string> GetLogAsync()
        {
            return Task.FromResult(string.Join(Environment.NewLine, _log));
        }

        public Task LogNoNotificationsFoundAsync()
        {
            return LogAsync("\nConclusion: No new messages were found. Replying with NoContent.");
        }

        public Task LogNoResponseAsync()
        {
            return LogAsync("\nConclusion: There is a message ready, but the domain did not provide it. Lying per contract and replying with NoContent.");
        }

        private Task LogAsync(string msg)
        {
#pragma warning disable CA2254
            _logger.LogInformation(msg);
#pragma warning restore CA2254
            _log.Enqueue(msg);
            return Task.CompletedTask;
        }
    }
}
