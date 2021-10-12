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
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Model.Logging;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Domain.Services;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public class PeekHandler :
        IRequestHandler<PeekCommand, PeekResponse>,
        IRequestHandler<PeekChargesCommand, PeekResponse>,
        IRequestHandler<PeekMasterDataCommand, PeekResponse>,
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

        public Task<PeekResponse> Handle(PeekChargesCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(request);
        }

        public Task<PeekResponse> Handle(PeekMasterDataCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(request);
        }

        public Task<PeekResponse> Handle(PeekAggregationsOrTimeSeriesCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(request);
        }

        private async Task<PeekResponse> HandleAsync(PeekCommandBase request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Func<MarketOperator, Uuid, Task<Bundle?>> requestHandler = request switch
            {
                PeekCommand => _marketOperatorDataDomainService.GetNextUnacknowledgedAsync,
                PeekChargesCommand => _marketOperatorDataDomainService.GetNextUnacknowledgedChargesAsync,
                PeekMasterDataCommand => _marketOperatorDataDomainService.GetNextUnacknowledgedMasterDataAsync,
                PeekAggregationsOrTimeSeriesCommand => _marketOperatorDataDomainService.GetNextUnacknowledgedAggregationsOrTimeSeriesAsync,
                _ => throw new ArgumentOutOfRangeException(nameof(request))
            };

            var marketOperator = new MarketOperator(new GlobalLocationNumber(request.MarketOperator));
            var uuid = new Uuid(request.BundleId);
            var bundle = await requestHandler(marketOperator, uuid).ConfigureAwait(false);

            IBundleContent? bundleContent = null;

            var bundleToReturn = bundle != null && bundle.TryGetContent(out bundleContent)
                ? new PeekResponse(true, await bundleContent.OpenAsync().ConfigureAwait(false))
                : new PeekResponse(false, Stream.Null);

            if (bundleToReturn.HasContent)
            {
                await _log.SaveLogOccurrenceAsync(
                        new Log(
                            request.GetType().Name,
                            new GlobalLocationNumber(request.MarketOperator),
                            request.MarketOperator + "+" + request.BundleId,
                            bundleContent!))
                    .ConfigureAwait(false);
            }

            var bundleId = new Uuid(request.BundleId);
            var bundleToPrepare = await requestHandler(marketOperator, bundleId).ConfigureAwait(false);

            return bundleToPrepare != null && bundleToPrepare.TryGetContent(out bundleContent)
                ? new PeekResponse(true, await bundleContent.OpenAsync().ConfigureAwait(false))
                : new PeekResponse(false, Stream.Null);
        }
    }
}
