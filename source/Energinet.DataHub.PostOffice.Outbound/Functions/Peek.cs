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
using System.Net.Http;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Outbound.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public class Peek
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public Peek(
            IMediator mediator,
            ILogger<GetMessage> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [Function("Peek")]
        public async Task<HttpResponseMessage> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get")]
            HttpRequestData request,
            FunctionContext context)
        {
            var logger = context.GetLogger(nameof(GetMessage));
            var command = request.GetPeekCommand();

            logger.LogInformation($"Processing GetMessage query: {command}.");

            var (hasContent, stream) = await _mediator.Send(command).ConfigureAwait(false);

            return hasContent
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) }
                : new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}