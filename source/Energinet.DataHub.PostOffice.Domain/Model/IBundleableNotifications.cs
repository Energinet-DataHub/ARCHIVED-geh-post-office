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
    /// Enqueue data available-notifications which are able to be bundled.
    /// </summary>
    public interface IBundleableNotifications
    {
        /// <summary>
        /// Contains all notifications
        /// </summary>
        IEnumerable<DataAvailableNotification> Notifications { get; }

        /// <summary>
        /// Key to identify similar type of messages to bundle.
        /// </summary>
        BundleableNotificationsKey PartitionKey { get; set; }

        /// <summary>
        /// Send data available-notifications to a queue.
        /// </summary>
        /// <param name="notification">Data available-notification to be enqueued.</param>
        void AddNotification(DataAvailableNotification notification);
    }
}
