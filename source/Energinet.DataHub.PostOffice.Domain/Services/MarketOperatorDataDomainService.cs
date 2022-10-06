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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using DomainOrigin = Energinet.DataHub.PostOffice.Domain.Model.DomainOrigin;

namespace Energinet.DataHub.PostOffice.Domain.Services
{
    public sealed class MarketOperatorDataDomainService : IMarketOperatorDataDomainService
    {
        private readonly IBundleRepository _bundleRepository;
        private readonly IDataAvailableNotificationRepository _dataAvailableNotificationRepository;
        private readonly IRequestBundleDomainService _requestBundleDomainService;
        private readonly IWeightCalculatorDomainService _weightCalculatorDomainService;

        public MarketOperatorDataDomainService(
            IBundleRepository bundleRepository,
            IDataAvailableNotificationRepository dataAvailableRepository,
            IRequestBundleDomainService requestBundleDomainService,
            IWeightCalculatorDomainService weightCalculatorDomainService)
        {
            _bundleRepository = bundleRepository;
            _dataAvailableNotificationRepository = dataAvailableRepository;
            _requestBundleDomainService = requestBundleDomainService;
            _weightCalculatorDomainService = weightCalculatorDomainService;
        }

        public Task<Bundle?> GetNextUnacknowledgedAsync(
            ActorId recipient,
            Uuid? suggestedBundleId,
            ResponseFormat responseFormat,
            double responseVersion)
        {
            return GetNextUnacknowledgedForDomainsAsync(
                recipient,
                suggestedBundleId,
                responseFormat,
                responseVersion);
        }

        public Task<Bundle?> GetNextUnacknowledgedTimeSeriesAsync(
            ActorId recipient,
            Uuid? suggestedBundleId,
            ResponseFormat responseFormat,
            double responseVersion)
        {
            return GetNextUnacknowledgedForDomainsAsync(
                recipient,
                suggestedBundleId,
                responseFormat,
                responseVersion,
                DomainOrigin.TimeSeries);
        }

        public Task<Bundle?> GetNextUnacknowledgedAggregationsAsync(
            ActorId recipient,
            Uuid? suggestedBundleId,
            ResponseFormat responseFormat,
            double responseVersion)
        {
            return GetNextUnacknowledgedForDomainsAsync(
                recipient,
                suggestedBundleId,
                responseFormat,
                responseVersion,
                DomainOrigin.Aggregations);
        }

        public Task<Bundle?> GetNextUnacknowledgedMasterDataAsync(
            ActorId recipient,
            Uuid? suggestedBundleId,
            ResponseFormat responseFormat,
            double responseVersion)
        {
            return GetNextUnacknowledgedForDomainsAsync(
                recipient,
                suggestedBundleId,
                responseFormat,
                responseVersion,
                DomainOrigin.MarketRoles,
                DomainOrigin.MeteringPoints,
                DomainOrigin.Charges);
        }

        public async Task<(bool CanAcknowledge, Bundle? Bundle)> CanAcknowledgeAsync(ActorId recipient, Uuid bundleId)
        {
            var bundle = await _bundleRepository.GetAsync(recipient, bundleId).ConfigureAwait(false);
            return bundle is { Dequeued: false }
                ? (true, bundle)
                : (false, null);
        }

        public async Task AcknowledgeAsync(Bundle bundle)
        {
            ArgumentNullException.ThrowIfNull(bundle, nameof(bundle));

            await _dataAvailableNotificationRepository
                .AcknowledgeAsync(bundle)
                .ConfigureAwait(false);

            await _bundleRepository
                .AcknowledgeAsync(bundle.Recipient, bundle.BundleId)
                .ConfigureAwait(false);
        }

        private async Task<Bundle?> GetNextUnacknowledgedForDomainsAsync(
            ActorId recipient,
            Uuid? suggestedBundleId,
            ResponseFormat responseFormat,
            double responseVersion,
            params DomainOrigin[] domains)
        {
            var existingBundle = await _bundleRepository
                .GetNextUnacknowledgedAsync(recipient, domains)
                .ConfigureAwait(false);

            if (existingBundle != null)
            {
                if (suggestedBundleId != null &&
                    suggestedBundleId != existingBundle.BundleId)
                {
                    throw new ValidationException(
                        $"The specified bundle id was rejected, as the current bundle {existingBundle.BundleId} is yet to be acknowledged.");
                }

                if (responseFormat != existingBundle.ResponseFormat)
                {
                    throw new ValidationException(
                        $"The specified bundle response format was rejected, as the current bundle {existingBundle.BundleId} was already requested with another format.");
                }

                return await AskSubDomainForContentAsync(existingBundle, responseFormat, responseVersion).ConfigureAwait(false);
            }

            var cabinetReader = await _dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(recipient, domains)
                .ConfigureAwait(false);

            // Nothing to return.
            if (cabinetReader == null)
                return null;

            var newBundle = await CreateNextBundleAsync(suggestedBundleId, cabinetReader, responseFormat).ConfigureAwait(false);

            var bundleCreatedResponse = await _bundleRepository
                .TryAddNextUnacknowledgedAsync(newBundle, cabinetReader)
                .ConfigureAwait(false);

            return bundleCreatedResponse switch
            {
                BundleCreatedResponse.Success => await AskSubDomainForContentAsync(newBundle, responseFormat, responseVersion).ConfigureAwait(false),
                BundleCreatedResponse.AnotherBundleExists => null,
                BundleCreatedResponse.BundleIdAlreadyInUse => throw new ValidationException(nameof(BundleCreatedResponse.BundleIdAlreadyInUse)),
                _ => throw new InvalidOperationException($"bundleCreatedResponse was {bundleCreatedResponse}")
            };
        }

        private async Task<Bundle?> AskSubDomainForContentAsync(Bundle bundle, ResponseFormat responseFormat, double responseVersion)
        {
            if (bundle.TryGetContent(out _))
                return bundle;

            var bundleContent = await _requestBundleDomainService
                .WaitForBundleContentFromSubDomainAsync(bundle, responseFormat, responseVersion)
                .ConfigureAwait(false);

            if (bundleContent == null)
                return bundle; // Timeout or error. Currently returned as "no new data".

            bundle.AssignContent(bundleContent);
            await _bundleRepository.SaveAsync(bundle).ConfigureAwait(false);
            return bundle;
        }

        private async Task<Bundle> CreateNextBundleAsync(Uuid? suggestedBundleId, ICabinetReader cabinetReader, ResponseFormat responseFormat)
        {
            var cabinetKey = cabinetReader.Key;

            var weight = new Weight(0);
            var maxWeight = _weightCalculatorDomainService.CalculateMaxWeight(cabinetKey.Origin);

            var notificationIds = new List<Uuid>();
            var documentTypes = new HashSet<string>();

            while (cabinetReader.CanPeek)
            {
                if (notificationIds.Count == 0)
                {
                    // Initial notification is always taken.
                    // If the weight is too high, a bundle is created anyway, with just this notification.
                    var notification = await cabinetReader.TakeAsync().ConfigureAwait(false);

                    weight += notification.Weight;
                    notificationIds.Add(notification.NotificationId);
                    documentTypes.Add(notification.DocumentType.Value);

                    if (!notification.SupportsBundling.Value)
                        break;
                }
                else
                {
                    var notification = cabinetReader.Peek();
                    if (notification.SupportsBundling.Value && weight + notification.Weight <= maxWeight)
                    {
                        var dequeued = await cabinetReader
                            .TakeAsync()
                            .ConfigureAwait(false);

                        weight += dequeued.Weight;
                        notificationIds.Add(dequeued.NotificationId);
                        documentTypes.Add(dequeued.DocumentType.Value);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return new Bundle(
                suggestedBundleId ?? new Uuid(),
                cabinetKey.Recipient,
                cabinetKey.Origin,
                cabinetKey.ContentType,
                notificationIds,
                documentTypes,
                responseFormat);
        }
    }
}
