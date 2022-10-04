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
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Actor = Energinet.DataHub.Core.App.Common.Abstractions.Actor.Actor;

namespace Energinet.DataHub.PostOffice.Common.Auth;

public sealed class LegacyActorProviderProxy : IActorProvider
{
    private readonly LegacyActorProvider _legacyActorProvider;
    private readonly LegacyActorIdIdentity _legacyActorIdIdentity;
    private readonly ActorRegistryProvider _actorRegistryProvider;
    private readonly IMarketOperatorFlowLogger _marketOperatorFlowLogger;

    public LegacyActorProviderProxy(
        LegacyActorProvider legacyActorProvider,
        LegacyActorIdIdentity legacyActorIdIdentity,
        ActorRegistryProvider actorRegistryProvider,
        IMarketOperatorFlowLogger marketOperatorFlowLogger)
    {
        _legacyActorProvider = legacyActorProvider;
        _legacyActorIdIdentity = legacyActorIdIdentity;
        _actorRegistryProvider = actorRegistryProvider;
        _marketOperatorFlowLogger = marketOperatorFlowLogger;
    }

    public async Task<Actor> GetActorAsync(Guid actorId)
    {
        var registryActor = await GetRegistryActorAsync(actorId).ConfigureAwait(false);
        var legacyActor = await GetLegacyActorAsync(actorId).ConfigureAwait(false);

        await LogActorsFoundAsync(actorId, registryActor, legacyActor).ConfigureAwait(false);

        if (registryActor != null)
        {
            if (legacyActor != null)
            {
                _legacyActorIdIdentity.Identity = new LegacyActorId(new GlobalLocationNumber(legacyActor.Identifier));
            }

            return registryActor;
        }

        if (legacyActor != null)
        {
            return legacyActor;
        }

        throw new InvalidOperationException($"Actor with id {actorId} not found.");
    }

    private async Task<Actor?> GetRegistryActorAsync(Guid actorId)
    {
        try
        {
            return await _actorRegistryProvider
                .GetActorAsync(actorId)
                .ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private async Task<Actor?> GetLegacyActorAsync(Guid actorId)
    {
        try
        {
            return await _legacyActorProvider
                .GetActorAsync(actorId)
                .ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private async Task LogActorsFoundAsync(Guid actorId, Actor? registryActor, Actor? legacyActor)
    {
        if (registryActor != null)
            await _marketOperatorFlowLogger.LogActorFoundAsync(actorId).ConfigureAwait(false);
        else
            await _marketOperatorFlowLogger.LogActorNotFoundAsync(actorId).ConfigureAwait(false);

        if (legacyActor != null)
            await _marketOperatorFlowLogger.LogLegacyActorFoundAsync(actorId, legacyActor.Identifier).ConfigureAwait(false);
        else
            await _marketOperatorFlowLogger.LogLegacyActorNotFoundAsync(actorId).ConfigureAwait(false);
    }
}
