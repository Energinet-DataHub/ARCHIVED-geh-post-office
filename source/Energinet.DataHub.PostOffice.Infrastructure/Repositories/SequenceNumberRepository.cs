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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public class SequenceNumberRepository : ISequenceNumberRepository
    {
        private IDataAvailableNotificationRepositoryContainer _repositoryContainer;

        public SequenceNumberRepository(IDataAvailableNotificationRepositoryContainer repositoryContainer)
        {
            _repositoryContainer = repositoryContainer;
        }

        public async Task<SequenceNumber> GetCurrentAsync()
        {
            var asLinq = _repositoryContainer.Container.GetItemLinqQueryable<CosmosSequenceNumber>();

            var query = from sequenceNumber in asLinq
                where
                    sequenceNumber.Id == "1"
                select sequenceNumber;

            var cosmosSequenceNumber = await ExecuteQueryAsync(query).FirstOrDefaultAsync().ConfigureAwait(false);

            if (cosmosSequenceNumber is null)
                throw new InvalidOperationException(nameof(cosmosSequenceNumber));

            return new SequenceNumber(cosmosSequenceNumber.SequenceNumber);
        }

        private static async IAsyncEnumerable<T> ExecuteQueryAsync<T>(IQueryable<T> query)
        {
            await foreach (var document in query.AsCosmosIteratorAsync().ConfigureAwait(false))
            {
                yield return document;
            }
        }
    }
}
