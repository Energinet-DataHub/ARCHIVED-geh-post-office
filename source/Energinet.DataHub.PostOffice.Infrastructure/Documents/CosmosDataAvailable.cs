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

using System;
using Newtonsoft.Json;

namespace Energinet.DataHub.PostOffice.Infrastructure.Documents
{
    internal sealed record CosmosDataAvailable
    {
        public CosmosDataAvailable()
        {
            Id = Guid.NewGuid().ToString();
            Uuid = null!;
            ContentType = null!;
            Origin = null!;
            Recipient = null!;
            Timestamp = null!;
        }

        public string Id { get; init; }
        public string Uuid { get; init; }
        public string ContentType { get; init; }
        public string Origin { get; init; }
        public string Recipient { get; init; }
        public bool SupportsBundling { get; init; }
        public int RelativeWeight { get; init; }
        public bool Acknowledge { get; init; }

        [JsonProperty(PropertyName = "_ts")]
        public string Timestamp { get; init; }
    }
}
