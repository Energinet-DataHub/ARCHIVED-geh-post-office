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
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.PostOffice.Common.Auth
{
    public sealed class JwtAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IMarketOperatorIdentity _identity;
        private readonly IActorContext _actorContext;

        public JwtAuthenticationMiddleware(IMarketOperatorIdentity identity, IActorContext actorContext)
        {
            _identity = identity;
            _actorContext = actorContext;
        }

        public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            if (!_identity.HasIdentity && _actorContext.CurrentActor != null)
            {
                if (!string.IsNullOrWhiteSpace(_actorContext.CurrentActor.Identifier))
                {
                    _identity.AssignId(_actorContext.CurrentActor.Identifier);
                }
                else
                {
                    _identity.AssignId(_actorContext.CurrentActor.ActorId.ToString());
                }
            }

            return next(context);
        }
    }
}
