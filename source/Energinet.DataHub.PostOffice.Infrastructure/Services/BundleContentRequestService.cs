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
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.MessageHub.Core.Peek;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Extensions.Logging;
using DomainOrigin = Energinet.DataHub.MessageHub.Model.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services
{
    public sealed class BundleContentRequestService : IBundleContentRequestService
    {
        private readonly ILogger _logger;
        private readonly IMarketOperatorFlowLogger _marketOperatorFlowLogger;
        private readonly IMarketOperatorDataStorageService _marketOperatorDataStorageService;
        private readonly IDataBundleRequestSender _dataBundleRequestSender;
        private readonly ICorrelationContext _correlationContext;
        private readonly IMarketOperatorFlowLogger _flogger;

        public BundleContentRequestService(
            ILogger logger,
            IMarketOperatorFlowLogger marketOperatorFlowLogger,
            IMarketOperatorDataStorageService marketOperatorDataStorageService,
            IDataBundleRequestSender dataBundleRequestSender,
            ICorrelationContext correlationContext,
            IMarketOperatorFlowLogger flogger)
        {
            _logger = logger;
            _marketOperatorFlowLogger = marketOperatorFlowLogger;
            _marketOperatorDataStorageService = marketOperatorDataStorageService;
            _dataBundleRequestSender = dataBundleRequestSender;
            _correlationContext = correlationContext;
            _flogger = flogger;
        }

        public async Task<IBundleContent?> WaitForBundleContentFromSubDomainAsync(Bundle bundle, ResponseFormat responseFormat, double responseVersion)
        {
            ArgumentNullException.ThrowIfNull(bundle, nameof(bundle));

            var request = new DataBundleRequestDto(
                Guid.NewGuid(),
                bundle.ProcessId.ToString(),
                bundle.ProcessId.ToString(),
                new MessageTypeDto(bundle.ContentType.Value),
                responseFormat,
                responseVersion);

            _logger.LogProcess("Peek", "WaitForContent", _correlationContext.Id, bundle.Recipient.ToString(), bundle.BundleId.ToString(), bundle.Origin.ToString());
            await _marketOperatorFlowLogger.LogSubDomainOriginDataRequestAsync(bundle.Origin).ConfigureAwait(false);

            var response = await _dataBundleRequestSender.SendAsync(request, (DomainOrigin)bundle.Origin).ConfigureAwait(false);
            if (response == null)
            {
                await _flogger.LogRequestDataFromSubdomainTimeoutFoundAsync(request.IdempotencyId).ConfigureAwait(false);
                _logger.LogProcess("Peek", "NoDomainResponse", _correlationContext.Id, bundle.Recipient.ToString(), bundle.BundleId.ToString(), bundle.Origin.ToString());
                return null;
            }

            if (response.IsErrorResponse)
            {
                _logger.LogProcess("Peek", "DomainErrorResponse", _correlationContext.Id, bundle.Recipient.ToString(), bundle.BundleId.ToString(), bundle.Origin.ToString());
                _logger.LogError(
                    "Domain returned an error {Reason}. Correlation ID: {CorrelationId}.\nDescription: {FailureDescription}",
                    response.ResponseError.Reason,
                    _correlationContext.Id,
                    response.ResponseError.FailureDescription);
                return null;
            }

            _logger.LogProcess("Peek", "DomainResponse", _correlationContext.Id, bundle.Recipient.ToString(), bundle.BundleId.ToString(), bundle.Origin.ToString());

            return new AzureBlobBundleContent(_marketOperatorDataStorageService, response.ContentUri);
        }
    }
}
