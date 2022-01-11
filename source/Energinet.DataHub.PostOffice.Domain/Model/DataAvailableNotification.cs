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

namespace Energinet.DataHub.PostOffice.Domain.Model
{
    public sealed class DataAvailableNotification
    {
        public DataAvailableNotification(
            Uuid notificationId,
            MarketOperator recipient,
            ContentType contentType,
            DomainOrigin origin,
            SupportsBundling supportsBundling,
            Weight weight,
            SequenceNumber sequenceNumber)
        {
            NotificationId = notificationId;
            Recipient = recipient;
            ContentType = contentType;
            Origin = origin;
            SupportsBundling = supportsBundling;
            Weight = weight;
            SequenceNumber = sequenceNumber;
            PartitionKey = Recipient.Gln.Value + Origin + ContentType.Value;
        }

        public Uuid NotificationId { get; }
        public MarketOperator Recipient { get; }
        public ContentType ContentType { get; }
        public DomainOrigin Origin { get; }
        public SupportsBundling SupportsBundling { get; }
        public Weight Weight { get; }
        public SequenceNumber SequenceNumber { get; }
        public string PartitionKey { get; set; }
    }
}
