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

using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure.Model;

namespace Energinet.DataHub.PostOffice.Infrastructure.Repositories;

public sealed class LegacyDataAvailableNotificationRepositoryProxy : IDataAvailableNotificationRepository
{
    private readonly DataAvailableNotificationRepository _dataAvailableNotificationRepository;
    private readonly LegacyActorIdIdentity _legacyActorIdIdentity;

    public LegacyDataAvailableNotificationRepositoryProxy(
        DataAvailableNotificationRepository dataAvailableNotificationRepository,
        LegacyActorIdIdentity legacyActorIdIdentity)
    {
        _dataAvailableNotificationRepository = dataAvailableNotificationRepository;
        _legacyActorIdIdentity = legacyActorIdIdentity;
    }

    public async Task<ICabinetReader?> GetNextUnacknowledgedAsync(ActorId recipient, params DomainOrigin[] domains)
    {
        if (_legacyActorIdIdentity.Identity != null)
        {
            var legacy = await _dataAvailableNotificationRepository
                .GetNextUnacknowledgedAsync(_legacyActorIdIdentity.Identity, domains)
                .ConfigureAwait(false);

            if (legacy != null)
                return legacy;
        }

        return await _dataAvailableNotificationRepository
            .GetNextUnacknowledgedAsync(recipient, domains)
            .ConfigureAwait(false);
    }

    public Task SaveAsync(CabinetKey key, IReadOnlyList<DataAvailableNotification> notifications)
    {
        return _dataAvailableNotificationRepository.SaveAsync(key, notifications);
    }

    public Task AcknowledgeAsync(Bundle bundle)
    {
        return _dataAvailableNotificationRepository.AcknowledgeAsync(bundle);
    }
}
