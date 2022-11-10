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
using Energinet.DataHub.PostOffice.Infrastructure.Documents;

namespace Energinet.DataHub.PostOffice.Infrastructure.Services;

/// <summary>
/// Checks whether the specified notification has been received before.
/// </summary>
public interface IDataAvailableIdempotencyService
{
    /// <summary>
    /// Returns true if the specified notification has been received before; false otherwise.
    /// </summary>
    /// <param name="notification">The notification to check idempotency for.</param>
    /// <param name="destinationDrawer">The drawer the notification is to be placed into.</param>
    internal Task<bool> WasReceivedPreviouslyAsync(
        DataAvailableNotification notification,
        CosmosCabinetDrawer destinationDrawer);
}
