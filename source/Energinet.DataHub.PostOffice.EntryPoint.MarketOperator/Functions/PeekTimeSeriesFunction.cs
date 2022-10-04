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
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Common.Auth;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions.Helpers;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions
{
    public sealed class PeekTimeSeriesFunction
    {
        private readonly IMediator _mediator;
        private readonly IMarketOperatorIdentity _operatorIdentity;
        private readonly IExternalBundleIdProvider _bundleIdProvider;
        private readonly IExternalResponseFormatProvider _responseFormatProvider;
        private readonly IExternalResponseVersionProvider _responseVersionProvider;
        private readonly IMarketOperatorFlowLogHelper _marketOperatorFlowLogHelper;

        public PeekTimeSeriesFunction(
            IMediator mediator,
            IMarketOperatorIdentity operatorIdentity,
            IExternalBundleIdProvider bundleIdProvider,
            IExternalResponseFormatProvider responseFormatProvider,
            IExternalResponseVersionProvider responseVersionProvider,
            IMarketOperatorFlowLogHelper marketOperatorFlowLogHelper)
        {
            _mediator = mediator;
            _operatorIdentity = operatorIdentity;
            _bundleIdProvider = bundleIdProvider;
            _responseFormatProvider = responseFormatProvider;
            _responseVersionProvider = responseVersionProvider;
            _marketOperatorFlowLogHelper = marketOperatorFlowLogHelper;
        }

        [Function("PeekTimeSeries")]
        public Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "peek/timeseries")]
            HttpRequestData request)
        {
            return request.ProcessAsync(async () =>
            {
                var command = new PeekTimeSeriesCommand(
                    _operatorIdentity.ActorId,
                    _bundleIdProvider.TryGetBundleId(request),
                    _responseFormatProvider.TryGetResponseFormat(request),
                    _responseVersionProvider.TryGetResponseVersion(request));
                var (hasContent, bundleId, stream, documentTypes) = await _mediator.Send(command).ConfigureAwait(false);

                var response = hasContent
                    ? request.CreateResponse(stream, MediaTypeNames.Application.Xml)
                    : await _marketOperatorFlowLogHelper.GetFlowLogResponseAsync(request, HttpStatusCode.NoContent).ConfigureAwait(false);

                response.Headers.Add(Constants.BundleIdHeaderName, bundleId);
                response.Headers.Add(Constants.MessageTypeName, string.Join(",", documentTypes));

                return response;
            });
        }
    }
}
