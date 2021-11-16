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
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.PostOffice.Common.Auth
{
    public sealed class QueryAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IMarketOperatorIdentity _identity;

        public QueryAuthenticationMiddleware(IMarketOperatorIdentity identity)
        {
            _identity = identity;
        }

        public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (!_identity.HasIdentity)
            {
                if (context.BindingContext.BindingData["marketOperator"] is string gln)
                {
                    _identity.AssignGln(gln);
                }
            }

            return next(context);
        }
    }
}
