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
using Energinet.DataHub.PostOffice.Domain.Model.Logging;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly ILogRepositoryContainer _logRepositoryContainer;

        public LogRepository(ILogRepositoryContainer logRepositoryContainer)
        {
            _logRepositoryContainer = logRepositoryContainer;
        }

        public Task SavePeekLogOccurrenceAsync(PeekLog log)
        {
            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var instanceToLog = new CosmosLog(
                log.Id.ToString(),
                log.Timestamp,
                log.EndpointType,
                log.ProcessId.Recipient.Gln.Value,
                log.ProcessId.ToString(),
                log.BundleReference.LogIdentifier);

            return _logRepositoryContainer.Container.CreateItemAsync(instanceToLog);
        }

        public Task SaveDequeueLogOccurrenceAsync(DequeueLog log)
        {
            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var instanceToLog = new CosmosLog(
                log.Id.ToString(),
                log.Timestamp,
                log.EndpointType,
                log.ProcessId.Recipient.Gln.Value,
                log.ProcessId.ToString());

            return _logRepositoryContainer.Container.CreateItemAsync(instanceToLog);
        }
    }
}
