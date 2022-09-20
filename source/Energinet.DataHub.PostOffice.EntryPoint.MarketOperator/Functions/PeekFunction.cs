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

using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions.Helpers;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions
{
    public sealed class PeekFunction
    {
        private readonly IMediator _mediator;
        private readonly IMarketOperatorIdentity _operatorIdentity;
        private readonly IExternalBundleIdProvider _bundleIdProvider;
        private readonly IExternalResponseFormatProvider _responseFormatProvider;
        private readonly IExternalResponseVersionProvider _responseVersionProvider;

        public PeekFunction(
            IMediator mediator,
            IMarketOperatorIdentity operatorIdentity,
            IExternalBundleIdProvider bundleIdProvider,
            IExternalResponseFormatProvider responseFormatProvider,
            IExternalResponseVersionProvider responseVersionProvider)
        {
            _mediator = mediator;
            _operatorIdentity = operatorIdentity;
            _bundleIdProvider = bundleIdProvider;
            _responseFormatProvider = responseFormatProvider;
            _responseVersionProvider = responseVersionProvider;
        }

        [Function("Peek")]
        public Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequestData request)
        {
            return request.ProcessAsync(async () =>
            {
                var responseFormat = _responseFormatProvider.TryGetResponseFormat(request);
                var command = new PeekCommand(
                    _operatorIdentity.ActorId,
                    _bundleIdProvider.TryGetBundleId(request),
                    responseFormat,
                    _responseVersionProvider.TryGetResponseVersion(request));
                var (hasContent, bundleId, stream, documentTypes) = await _mediator.Send(command).ConfigureAwait(false);

                var response = hasContent
                    ? request.CreateResponse(stream,  responseFormat == ResponseFormat.Xml ? MediaTypeNames.Application.Xml : MediaTypeNames.Application.Json)
                    : request.CreateResponse(HttpStatusCode.NoContent);

                response.Headers.Add(Constants.BundleIdHeaderName, bundleId);
                response.Headers.Add(Constants.MessageTypeName, string.Join(",", documentTypes));

                return response;
            });
        }
    }
}
