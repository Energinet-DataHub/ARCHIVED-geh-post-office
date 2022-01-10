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

namespace Energinet.DataHub.PostOffice.Domain.Model
{
    /// <summary>
    /// Accesses data available notifications that can be bundled together
    /// </summary>
    public interface IBundleableNotificationsStorage
    {
        /// <summary>
        /// Contains the key to access notifications
        /// </summary>
        BundleableNotificationsKey BundleableNotificationsKey { get; set; }

        /// <summary>
        /// Flag to indicate if notifications can be peeked
        /// </summary>
        bool CanPeek { get; set; }

        /// <summary>
        /// Get next available notifications
        /// </summary>
        /// <returns>A collection of data available notifications</returns>
        IEnumerable<DataAvailableNotification> Peek();

        /// <summary>
        /// Marks notifications as processed and thereby makes them unable for a market actor to peek again
        /// </summary>
        /// <param name="recipient">The recipient of the notifications</param>
        /// <param name="bundleId">An identifier to locate all the bundled notifications</param>
        void Dequeue(MarketOperator recipient, Uuid bundleId);
    }
}
