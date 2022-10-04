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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services
{
    public class MarketOperatorFlowLogger : IMarketOperatorFlowLogger
    {
        private readonly ConcurrentQueue<string> _log = new ConcurrentQueue<string>();
        private readonly ILogger _logger;

        public MarketOperatorFlowLogger(ILogger logger)
        {
            _logger = logger;
        }

        public Task LogActorFoundAsync(Guid externalActorId, Guid actorId)
        {
            return LogAsync($"Actor found with external id '{externalActorId}' and id '{actorId}'");
        }

        public Task LogActorNotFoundAsync(Guid externalActorId)
        {
            return LogAsync($"An actor was not found with external id '{externalActorId}'");
        }

        public Task LogLegacyActorFoundAsync(Guid externalActorId, string gln)
        {
            return LogAsync($"Legacy actor found with external id '{externalActorId}' with GLN number '{gln}'");
        }

        public Task LogLegacyActorNotFoundAsync(Guid externalActorId)
        {
            return LogAsync($"A legacy actor was not found with external id '{externalActorId}'");
        }

        public Task LogSubDomainOriginDataRequestAsync(DomainOrigin origin)
        {
            return LogAsync($"Bundle data requested from subdomain: '{origin}'");
        }

        public Task<string> GetLogAsync()
        {
            return Task.FromResult(string.Join(Environment.NewLine, _log));
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
