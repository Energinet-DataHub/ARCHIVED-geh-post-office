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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public sealed class PeekHandler :
        IRequestHandler<PeekCommand, PeekResponse>,
        IRequestHandler<PeekTimeSeriesCommand, PeekResponse>,
        IRequestHandler<PeekMasterDataCommand, PeekResponse>,
        IRequestHandler<PeekAggregationsCommand, PeekResponse>
    {
        private readonly IMarketOperatorFlowLogger _marketOperatorFlowLogger;
        private readonly IMarketOperatorDataDomainService _marketOperatorDataDomainService;
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;

        public PeekHandler(
            IMarketOperatorFlowLogger marketOperatorFlowLogger,
            IMarketOperatorDataDomainService marketOperatorDataDomainService,
            ILogger logger,
            ICorrelationContext correlationContext)
        {
            _marketOperatorFlowLogger = marketOperatorFlowLogger;
            _marketOperatorDataDomainService = marketOperatorDataDomainService;
            _logger = logger;
            _correlationContext = correlationContext;
        }

        public Task<PeekResponse> Handle(PeekCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(request, _marketOperatorDataDomainService.GetNextUnacknowledgedAsync);
        }

        public Task<PeekResponse> Handle(PeekTimeSeriesCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(request, _marketOperatorDataDomainService.GetNextUnacknowledgedTimeSeriesAsync);
        }

        public Task<PeekResponse> Handle(PeekMasterDataCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(request, _marketOperatorDataDomainService.GetNextUnacknowledgedMasterDataAsync);
        }

        public Task<PeekResponse> Handle(PeekAggregationsCommand request, CancellationToken cancellationToken)
        {
            return HandleAsync(request, _marketOperatorDataDomainService.GetNextUnacknowledgedAggregationsAsync);
        }

        private async Task<PeekResponse> HandleAsync(
            PeekCommandBase request,
            Func<ActorId, Uuid?, ResponseFormat, double, Task<Bundle?>> requestHandler)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            _logger.LogProcess("Peek", _correlationContext.Id, request.MarketOperator);

            var marketOperator = Guid.TryParse(request.MarketOperator, out var actorId)
                ? new ActorId(actorId)
                : new LegacyActorId(new GlobalLocationNumber(request.MarketOperator));

            var suggestedBundleId = request.BundleId != null
                ? new Uuid(request.BundleId)
                : null;

            var bundle = await requestHandler(marketOperator, suggestedBundleId, request.ResponseFormat ?? ResponseFormat.Xml, request.ResponseVersion ?? 1.0).ConfigureAwait(false);

            if (bundle != null)
            {
                if (bundle.TryGetContent(out var bundleContent))
                {
                    _logger.LogProcess("Peek", "HasContent", _correlationContext.Id, request.MarketOperator, bundle.BundleId.ToString(), bundle.NotificationIds.Select(x => x.ToString()));

                    return new PeekResponse(
                        true,
                        bundle.BundleId.ToString(),
                        await bundleContent.OpenAsync().ConfigureAwait(false),
                        bundle.DocumentTypes);
                }

                await _marketOperatorFlowLogger
                    .LogNoResponseAsync()
                    .ConfigureAwait(false);

                _logger.LogProcess("Peek", "TimeoutOrError", _correlationContext.Id, request.MarketOperator, bundle.BundleId.ToString(), bundle.NotificationIds.Select(x => x.ToString()));
            }
            else
            {
                await _marketOperatorFlowLogger
                    .LogNoNotificationsFoundAsync()
                    .ConfigureAwait(false);
            }

            _logger.LogProcess("Peek", "NoContent", _correlationContext.Id, request.MarketOperator, string.Empty);

            return new PeekResponse(
                false,
                string.Empty,
                Stream.Null,
                Enumerable.Empty<string>());
        }
    }
}
