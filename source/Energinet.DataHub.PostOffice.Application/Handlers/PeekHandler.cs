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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Model.Logging;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using GreenEnergyHub.PostOffice.Communicator.Model;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public class PeekHandler :
        IRequestHandler<PeekCommand, PeekResponse>,
        IRequestHandler<PeekAggregationsOrTimeSeriesCommand, PeekResponse>
    {
        private readonly IMarketOperatorDataDomainService _marketOperatorDataDomainService;
        private ILogRepository _log;

        public PeekHandler(
            IMarketOperatorDataDomainService marketOperatorDataDomainService,
            ILogRepository log)
        {
            _marketOperatorDataDomainService = marketOperatorDataDomainService;
            _log = log;
        }

        public Task<PeekResponse> Handle(PeekCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(request);
        }

        public Task<PeekResponse> Handle(PeekAggregationsOrTimeSeriesCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(request);
        }

        private static async Task<PeekResponse> PrepareBundleAsync(Bundle? bundle)
        {
            return bundle != null && bundle.TryGetContent(out var bundleContent)
                ? new PeekResponse(true, await bundleContent.OpenAsync().ConfigureAwait(false))
                : new PeekResponse(false, Stream.Null);
        }

        private async Task<PeekResponse> HandleAsync(PeekCommandBase request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var logReferenceId = await _log.SaveLogOccurrenceAsync(
                    new Log(
                        endpointType: request.GetType().Name,
                        gln: new GlobalLocationNumber(request.Recipient),
                        processId: "processId", // TODO: Should be a value passed from the caller/market operator.
                        description: "Endpoint was called."))
                .ConfigureAwait(false);

            Func<MarketOperator, Task<Bundle?>> requestHandler = request switch
            {
                PeekCommand => _marketOperatorDataDomainService.GetNextUnacknowledgedAsync,
                PeekAggregationsOrTimeSeriesCommand => _marketOperatorDataDomainService.GetNextUnacknowledgedAggregationsOrTimeSeriesAsync,
                _ => throw new ArgumentOutOfRangeException(nameof(request))
            };

            var marketOperator = new MarketOperator(new GlobalLocationNumber(request.Recipient));
            var bundle = await requestHandler(marketOperator).ConfigureAwait(false);

            var bundleToReturn = await PrepareBundleAsync(bundle).ConfigureAwait(false);

            if (bundleToReturn.HasContent)
            {
                bundle!.TryGetContent(out var bundleContent);

                await _log.SaveLogOccurrenceAsync(
                        new Log(
                            endpointType: request.GetType().Name,
                            gln: new GlobalLocationNumber(request.Recipient),
                            processId: "processId", // TODO: Should be a value passed from the caller/market operator.
                            description: "Returning valid data.",
                            replyToMarketOperator: new Reply(
                                bundleReference: bundleContent!),
                            logReferenceId: logReferenceId))
                    .ConfigureAwait(false);
            }
            else
            {
                await _log.SaveLogOccurrenceAsync(
                        new Log(
                            endpointType: request.GetType().Name,
                            gln: new GlobalLocationNumber(request.Recipient),
                            processId: "processId", // TODO: Should be a value passed from the caller/market operator.
                            description: "Returning no data.",
                            new Reply(new DataBundleResponseErrorDto
                            {
                                Reason = DataBundleResponseErrorReason.DatasetNotFound
                            }),
                            logReferenceId: logReferenceId))
                    .ConfigureAwait(false);
            }

            return bundleToReturn;
        }
    }
}
