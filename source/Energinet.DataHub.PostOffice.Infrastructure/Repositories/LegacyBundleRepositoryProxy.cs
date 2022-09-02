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

using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Model;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories;

public sealed class LegacyBundleRepositoryProxy : IBundleRepository
{
    private readonly BundleRepository _bundleRepository;
    private readonly LegacyActorIdIdentity _legacyActorIdIdentity;

    public LegacyBundleRepositoryProxy(
        BundleRepository bundleRepository,
        LegacyActorIdIdentity legacyActorIdIdentity)
    {
        _bundleRepository = bundleRepository;
        _legacyActorIdIdentity = legacyActorIdIdentity;
    }

    public async Task<Bundle?> GetAsync(ActorId recipient, Uuid bundleId)
    {
        if (_legacyActorIdIdentity.Identity != null)
        {
            var legacy = await _bundleRepository
                .GetAsync(_legacyActorIdIdentity.Identity, bundleId)
                .ConfigureAwait(false);

            if (legacy != null)
                return legacy;
        }

        return await _bundleRepository
            .GetAsync(recipient, bundleId)
            .ConfigureAwait(false);
    }

    public async Task<Bundle?> GetNextUnacknowledgedAsync(ActorId recipient, params DomainOrigin[] domains)
    {
        if (_legacyActorIdIdentity.Identity != null)
        {
            var legacy = await _bundleRepository
                .GetNextUnacknowledgedAsync(_legacyActorIdIdentity.Identity, domains)
                .ConfigureAwait(false);

            if (legacy != null)
                return legacy;
        }

        return await _bundleRepository
            .GetNextUnacknowledgedAsync(recipient, domains)
            .ConfigureAwait(false);
    }

    public Task<BundleCreatedResponse> TryAddNextUnacknowledgedAsync(Bundle bundle, ICabinetReader cabinetReader)
    {
        return _bundleRepository.TryAddNextUnacknowledgedAsync(bundle, cabinetReader);
    }

    public Task AcknowledgeAsync(ActorId recipient, Uuid bundleId)
    {
        return _bundleRepository.AcknowledgeAsync(recipient, bundleId);
    }

    public Task SaveAsync(Bundle bundle)
    {
        return _bundleRepository.SaveAsync(bundle);
    }
}
