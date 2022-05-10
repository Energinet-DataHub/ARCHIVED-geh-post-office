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
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Actor = Energinet.DataHub.Core.App.Common.Abstractions.Actor.Actor;

namespace Energinet.DataHub.PostOffice.Common.Auth;

public sealed class LegacyActorProviderProxy : IActorProvider
{
    private readonly LegacyActorProvider _legacyActorProvider;
    private readonly LegacyActorIdIdentity _legacyActorIdIdentity;
    private readonly ActorRegistryProvider _actorRegistryProvider;

    public LegacyActorProviderProxy(
        LegacyActorProvider legacyActorProvider,
        LegacyActorIdIdentity legacyActorIdIdentity,
        ActorRegistryProvider actorRegistryProvider)
    {
        _legacyActorProvider = legacyActorProvider;
        _legacyActorIdIdentity = legacyActorIdIdentity;
        _actorRegistryProvider = actorRegistryProvider;
    }

    public async Task<Actor> GetActorAsync(Guid actorId)
    {
        var registryActor = await GetRegistryActorAsync(actorId).ConfigureAwait(false);
        var legacyActor = await GetLegacyActorAsync(actorId).ConfigureAwait(false);

        if (registryActor != null && legacyActor != null)
        {
            _legacyActorIdIdentity.Identity = new LegacyActorId(new GlobalLocationNumber(legacyActor.Identifier));
            return registryActor;
        }

        if (registryActor != null)
        {
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
}
