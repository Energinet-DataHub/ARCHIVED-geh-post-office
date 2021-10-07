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
    public class PeekHandler : IRequestHandler<PeekCommand, PeekResponse>
    {
        private const string EndpointType = "All";
        private readonly IMarketOperatorDataDomainService _marketOperatorDataDomainService;
        private ILogRepository _log;

        public PeekHandler(
            IMarketOperatorDataDomainService marketOperatorDataDomainService,
            ILogRepository log)
        {
            _marketOperatorDataDomainService = marketOperatorDataDomainService;
            _log = log;
        }

        public async Task<PeekResponse> Handle(PeekCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            await _log.SaveLogOccurrenceAsync(
                    new Log(
                        endpointType: EndpointType,
                        gln: new GlobalLocationNumber(request.Recipient),
                        processId: "processId", // TODO: Should be a value passed from the caller/market operator.
                        description: "Endpoint was called."))
                .ConfigureAwait(false);

            var bundle = await _marketOperatorDataDomainService
                .GetNextUnacknowledgedAsync(new MarketOperator(new GlobalLocationNumber(request.Recipient)))
                .ConfigureAwait(false);

            if (!(bundle != null && bundle.TryGetContent(out var bundleContent)))
            {
                await _log.SaveLogOccurrenceAsync(
                        new Log(
                            endpointType: EndpointType,
                            gln: new GlobalLocationNumber(request.Recipient),
                            processId: "processId", // TODO: Should be a value passed from the caller/market operator.
                            replyToMarketOperator: new Reply(
                                new DataBundleResponseErrorDto
                                {
                                    Reason = DataBundleResponseErrorReason.DatasetNotFound,
                                    FailureDescription = "No data bundle found."
                                }),
                            description: "Endpoint was called."))
                    .ConfigureAwait(false);

                return new PeekResponse(false, Stream.Null);
            }

            await _log.SaveLogOccurrenceAsync(
                    new Log(
                        endpointType: EndpointType,
                        gln: new GlobalLocationNumber(request.Recipient),
                        processId: "processId", // TODO: Should be a value passed from the caller/market operator.
                        replyToMarketOperator: new Reply(bundleContent),
                        description: "Endpoint was called."))
                .ConfigureAwait(false);

            return new PeekResponse(true, await bundleContent.OpenAsync().ConfigureAwait(false));
        }
    }
}
