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
        IRequestHandler<PeekTimeSeriesCommand, PeekResponse>,
        IRequestHandler<PeekMasterDataCommand, PeekResponse>,
        IRequestHandler<PeekAggregationsCommand, PeekResponse>
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
            return HandleAsync(
                request,
                _marketOperatorDataDomainService.GetNextUnacknowledgedAsync,
                (processId, bundleContent) => new PeekLog(processId, bundleContent));
        }

        public Task<PeekResponse> Handle(PeekTimeSeriesCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(
                request,
                _marketOperatorDataDomainService.GetNextUnacknowledgedChargesAsync,
                (processId, uuid) => new PeekChargesLog(processId, uuid));
        }

        public Task<PeekResponse> Handle(PeekMasterDataCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(
                request,
                _marketOperatorDataDomainService.GetNextUnacknowledgedMasterDataAsync,
                (processId, uuid) => new PeekMasterDataLog(processId, uuid));
        }

        public Task<PeekResponse> Handle(PeekAggregationsCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(
                request,
                _marketOperatorDataDomainService.GetNextUnacknowledgedAggregationsOrTimeSeriesAsync,
                (processId, uuid) => new PeekAggregationsOrTimeSeriesLog(processId, uuid));
        }

        private async Task<PeekResponse> HandleAsync(
            PeekCommandBase request,
            Func<MarketOperator, Uuid, Task<Bundle?>> requestHandler,
            Func<ProcessId, IBundleContent, PeekLog> logProvider)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var marketOperator = new MarketOperator(new GlobalLocationNumber(request.MarketOperator));
            var uuid = new Uuid(request.BundleId);
            var bundle = await requestHandler(marketOperator, uuid).ConfigureAwait(false);

            IBundleContent? bundleContent = null;

            var bundleToReturn = bundle != null && bundle.TryGetContent(out bundleContent)
                ? new PeekResponse(true, await bundleContent.OpenAsync().ConfigureAwait(false))
                : new PeekResponse(false, Stream.Null);

            if (bundleToReturn.HasContent)
            {
                var processId = new ProcessId(uuid, marketOperator);

                await _log.SavePeekLogOccurrenceAsync(logProvider(processId, bundleContent!)).ConfigureAwait(false);
            }

            return bundleToReturn;
        }
    }
}
