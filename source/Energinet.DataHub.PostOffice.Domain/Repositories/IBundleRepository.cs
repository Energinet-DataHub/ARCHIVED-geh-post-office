﻿// Copyright 2020 Energinet DataHub A/S
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

namespace Energinet.DataHub.PostOffice.Domain.Repositories
{
    /// <summary>
    /// Provides access to the bundles.
    /// </summary>
    public interface IBundleRepository
    {
        /// <summary>
        /// Gets the next bundle from the recipient that has yet to be acknowledged.
        /// </summary>
        /// <param name="recipient">The market operator to retrieve the next bundle for.</param>
        /// <returns>The next unacknowledged bundle; or null, if none is available.</returns>
        Task<Bundle?> GetNextUnacknowledgedAsync(MarketOperator recipient);

        /// <summary>
        /// Acknowledges the bundle with the specified bundle id.
        /// </summary>
        /// <param name="bundleId">The bundle id to acknowledge.</param>
        Task AcknowledgeAsync(Uuid bundleId);

        /// <summary>
        /// Adds the specified bundle as the next unacknowledged bundle,
        /// ensuring that only one bundle can be unacknowledged at a time.
        /// </summary>
        /// <param name="bundle">The bundle to add.</param>
        /// <returns>Returns true if the bundle was successfully added; false, if another bundle aready exists.</returns>
        Task<BundleCreatedResponse> TryAddNextUnacknowledgedAsync(Bundle bundle);

        /// <summary>
        /// Saves the bundle.
        /// </summary>
        /// <param name="bundle">The bundle to save.</param>
        Task SaveAsync(Bundle bundle);
    }
}
