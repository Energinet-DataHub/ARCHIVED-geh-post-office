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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Infrastructure.Documents;

namespace Energinet.DataHub.PostOffice.Infrastructure.Mappers;

internal static class ActorMapper
{
    public static CosmosActor Map(Actor actor)
    {
        var externalId = actor.ExternalId.Value.ToString();
        return new CosmosActor
        {
            Id = actor.Id.Value.ToString(),
            ExternalId = externalId,
            PartitionKey = actor.Id.Value.ToString()
        };
    }

    public static Actor Map(CosmosActor actor)
    {
        var actorId = new ActorId(Guid.Parse(actor.Id));
        var externalId = new ExternalActorId(Guid.Parse(actor.ExternalId));
        return new Actor(actorId, externalId);
    }
}
