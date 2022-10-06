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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.MessageHub.Core.Dequeue;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;
using DomainOrigin = Energinet.DataHub.MessageHub.Model.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public sealed class DequeueHandler : IRequestHandler<DequeueCommand, DequeueResponse>
    {
        private readonly IMarketOperatorDataDomainService _marketOperatorDataDomainService;
        private readonly IDequeueNotificationSender _dequeueNotificationSender;
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;
        private readonly IMarketOperatorFlowLogger _marketOperatorFlowLogger;

        public DequeueHandler(
            IMarketOperatorDataDomainService marketOperatorDataDomainService,
            IDequeueNotificationSender dequeueNotificationSender,
            ILogger logger,
            ICorrelationContext correlationContext,
            IMarketOperatorFlowLogger marketOperatorFlowLogger)
        {
            _marketOperatorDataDomainService = marketOperatorDataDomainService;
            _dequeueNotificationSender = dequeueNotificationSender;
            _logger = logger;
            _correlationContext = correlationContext;
            _marketOperatorFlowLogger = marketOperatorFlowLogger;
        }

        public async Task<DequeueResponse> Handle(DequeueCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            _logger.LogProcess("Dequeue", _correlationContext.Id, request.MarketOperator);

            await _marketOperatorFlowLogger
                .LogIntroAsync()
                .ConfigureAwait(false);

            var bundleId = new Uuid(request.BundleId);
            var recipient = Guid.TryParse(request.MarketOperator, out var actorId)
                ? new ActorId(actorId)
                : new LegacyActorId(new GlobalLocationNumber(request.MarketOperator));

            await _marketOperatorFlowLogger
                .LogActorDequeueingAsync(recipient.Value, _correlationContext.Id, request.BundleId)
                .ConfigureAwait(false);

            var (canAcknowledge, bundle) = await _marketOperatorDataDomainService
                .CanAcknowledgeAsync(recipient, bundleId)
                .ConfigureAwait(false);

            if (!canAcknowledge)
            {
                _logger.LogProcess("Dequeue", "Unacknowledged", _correlationContext.Id, request.MarketOperator, request.BundleId);
                return new DequeueResponse(false);
            }

            var marketOperator = recipient is LegacyActorId legacyActor
#pragma warning disable CS0618
                ? new LegacyActorIdDto(legacyActor.Value)
#pragma warning restore CS0618
                : new ActorIdDto(Guid.Parse(recipient.Value));

            var dequeueNotification = new DequeueNotificationDto(
                bundle!.ProcessId.ToString(),
                marketOperator);

            await _dequeueNotificationSender
                .SendAsync(bundle.ProcessId.ToString(), dequeueNotification, (DomainOrigin)bundle.Origin)
                .ConfigureAwait(false);

            await _marketOperatorDataDomainService
                .AcknowledgeAsync(bundle)
                .ConfigureAwait(false);

            _logger.LogProcess("Dequeue", "Acknowledged", _correlationContext.Id, request.MarketOperator, request.BundleId);
            return new DequeueResponse(true);
        }
    }
}
