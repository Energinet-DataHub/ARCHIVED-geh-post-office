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

using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;

namespace Energinet.DataHub.PostOffice.Domain.Repositories
{
    /// <summary>
    /// Stores actors from the Market Participant domain.
    /// </summary>
    public interface IActorRepository
    {
        /// <summary>
        /// Gets the actor with the specified actor id.
        /// </summary>
        /// <param name="actorId">The id of the actor to get.</param>
        /// <returns>The actor with the specified actor id; or null, if an actor was not found.</returns>
        Task<Actor?> GetActorAsync(ActorId actorId);

        /// <summary>
        /// Gets the actor with the specified external actor id.
        /// </summary>
        /// <param name="externalActorId">The external id of the actor to get.</param>
        /// <returns>The actor with the specified external actor id; or null, if an actor was not found.</returns>
        Task<Actor?> GetActorAsync(ExternalActorId externalActorId);

        /// <summary>
        /// Adds or updates the given actor in the database.
        /// </summary>
        /// <param name="actor">The actor to add or update.</param>
        Task AddOrUpdateAsync(Actor actor);

        /// <summary>
        /// Removes the given actor from the database.
        /// </summary>
        /// <param name="actor">The actor to remove.</param>
        Task DeleteAsync(Actor actor);
    }
}
