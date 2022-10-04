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
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using Actor = Energinet.DataHub.Core.App.Common.Abstractions.Actor.Actor;

namespace Energinet.DataHub.PostOffice.Common.Auth;

public sealed class ActorRegistryProvider : IActorProvider
{
    private readonly IActorRepository _actorRepository;
    private readonly IMarketOperatorFlowLogger _marketOperatorFlowLogger;

    public ActorRegistryProvider(IActorRepository actorRepository, IMarketOperatorFlowLogger marketOperatorFlowLogger)
    {
        _actorRepository = actorRepository;
        _marketOperatorFlowLogger = marketOperatorFlowLogger;
    }

    public async Task<Actor> GetActorAsync(Guid actorId)
    {
        var actor = await _actorRepository
            .GetActorAsync(new ExternalActorId(actorId))
            .ConfigureAwait(false);

        if (actor == null)
        {
            await _marketOperatorFlowLogger.LogActorNotFoundAsync(actorId).ConfigureAwait(false);
            throw new InvalidOperationException($"Actor with id {actorId} not found.");
        }

        await _marketOperatorFlowLogger.LogActorFoundAsync(actorId, Guid.Parse(actor.Id.Value)).ConfigureAwait(false);

        return new Actor(
            Guid.Parse(actor.Id.Value),
            string.Empty,
            string.Empty,
            string.Empty);
    }
}
