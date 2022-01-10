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
using Newtonsoft.Json;

namespace Energinet.DataHub.PostOffice.Infrastructure.Documents
{
    public sealed record CosmosDomainMessageType
    {
        public CosmosDomainMessageType(
            string recipient,
            string origin,
            string contentType,
            int sequenceNumber)
        {
            Recipient = recipient;
            Origin = origin;
            ContentType = contentType;
            SequenceNumber = sequenceNumber;
        }

        [JsonProperty("id")]
        public static Guid Id => Guid.NewGuid();
        public string Recipient { get; set; }
        public string Origin { get; set; }
        public string ContentType { get; set; }
        public int SequenceNumber { get; set; }
    }
}
