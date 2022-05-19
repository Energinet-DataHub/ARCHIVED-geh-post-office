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

namespace Energinet.DataHub.MessageHub.Model.Model
{
    public sealed record DataAvailableNotificationDto
    {
        /// <summary>
        /// Specifies which data is available for consumption by a market operator.
        /// When a notification is received, the data is immediately made available for peeking.
        /// </summary>
        /// <param name="uuid">
        /// A guid uniquely identifying the data. This guid will be passed back
        /// to the sub-domain with the request for data to be generated.
        /// </param>
        /// <param name="recipient">
        /// The id of the actor that can consume this data.
        /// This ActorId is provided by the MarketParticipant domain, and should not be confused with ExternalActorId.
        /// </param>
        /// <param name="messageType">
        /// A unique case-insensitive identification of the type of data.
        /// Data with matching types can be bundled together.
        /// </param>
        /// <param name="documentType">
        /// The RSM document type.
        /// Returned to market operators to help identify the contents of the data.
        /// </param>
        /// <param name="origin">
        /// An enum identifying the source domain.<br />
        /// - Market operators can request data from a specific origin (domain).<br />
        /// - When data has to be generated, the request will be sent to the specified origin (domain).
        /// </param>
        /// <param name="supportsBundling">
        /// Allows bundling this data with other data with an identical <paramref name="messageType" />.
        /// <paramref name="relativeWeight" /> has no meaning, if bundling is disabled.
        /// </param>
        /// <param name="relativeWeight">
        /// The weight of the current data. The weight is used to create bundles, where
        /// <c>∑(RelativeWeight) ≤ MaxWeight</c>. MaxWeight is specified by sub-domain.<br/>
        /// The weight and maximum weight are used to ensure
        /// that the resulting bundle stays within the data size limit.
        /// </param>
        public DataAvailableNotificationDto(
            Guid uuid,
            ActorIdDto recipient,
            MessageTypeDto messageType,
            string documentType,
            DomainOrigin origin,
            bool supportsBundling,
            int relativeWeight)
        {
            if (uuid == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(uuid));

            ArgumentNullException.ThrowIfNull(recipient, nameof(recipient));
            ArgumentNullException.ThrowIfNull(messageType, nameof(messageType));
            ArgumentNullException.ThrowIfNull(documentType, nameof(documentType));

            Uuid = uuid;
            Recipient = recipient;
            MessageType = messageType;
            DocumentType = documentType;
            Origin = origin;
            SupportsBundling = supportsBundling;
            RelativeWeight = relativeWeight;
        }

        public Guid Uuid { get; }
        public ActorIdDto Recipient { get; }
        public MessageTypeDto MessageType { get; }
        public string DocumentType { get; }
        public DomainOrigin Origin { get; }
        public bool SupportsBundling { get; }
        public int RelativeWeight { get; }
    }
}
