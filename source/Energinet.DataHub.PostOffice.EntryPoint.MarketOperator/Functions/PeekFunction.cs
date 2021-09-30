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

using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Common.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions
{
    public sealed class PeekFunction
    {
        private readonly IMediator _mediator;

        public PeekFunction(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("Peek")]
        public Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get")]
            HttpRequestData request)
        {
            return request.ProcessAsync(async () =>
            {
                var command = request.Url.ParseQuery<PeekCommand>();
                var (hasContent, stream) = await _mediator.Send(command).ConfigureAwait(false);
                return hasContent
                    ? request.CreateResponse(stream)
                    : request.CreateResponse(HttpStatusCode.NoContent);
            });
        }
    }
}