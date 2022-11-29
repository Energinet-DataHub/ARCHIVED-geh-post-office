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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Common;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories.Containers;
using Microsoft.Azure.Cosmos;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories
{
    public sealed class ActorRepository : IActorRepository
    {
        private readonly IActorRepositoryContainer _repositoryContainer;

        public ActorRepository(IActorRepositoryContainer repositoryContainer)
        {
            _repositoryContainer = repositoryContainer;
        }

        public async Task<Actor?> GetActorAsync(ActorId actorId)
        {
            ArgumentNullException.ThrowIfNull(actorId, nameof(actorId));

            var query =
                from actor in _repositoryContainer.Container.GetItemLinqQueryable<CosmosActor>()
                where actor.Id == actorId.Value.ToString()
                select actor;

            var actorDocument = await query
                .AsCosmosIteratorAsync()
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);

            return actorDocument != null ? ActorMapper.Map(actorDocument) : null;
        }

        public async Task<Actor?> GetActorAsync(ExternalActorId externalActorId)
        {
            ArgumentNullException.ThrowIfNull(externalActorId, nameof(externalActorId));

            var query =
                from actor in _repositoryContainer.Container.GetItemLinqQueryable<CosmosActor>()
                where actor.ExternalId == externalActorId.Value.ToString()
                select actor;

            var actorDocument = await query
                .AsCosmosIteratorAsync()
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);

            return actorDocument != null ? ActorMapper.Map(actorDocument) : null;
        }

        public Task AddOrUpdateAsync(Actor actor)
        {
            ArgumentNullException.ThrowIfNull(actor, nameof(actor));

            var actorDocument = ActorMapper.Map(actor);
            return _repositoryContainer
                .Container
                .UpsertItemAsync(actorDocument);
        }

        public async Task DeleteAsync(Actor actor)
        {
            ArgumentNullException.ThrowIfNull(actor, nameof(actor));

            var actorDocument = ActorMapper.Map(actor);

            try
            {
                await _repositoryContainer
                    .Container
                    .DeleteItemAsync<CosmosActor>(actorDocument.Id, new PartitionKey(actorDocument.Id))
                    .ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Nothing to do.
            }
        }
    }
}
