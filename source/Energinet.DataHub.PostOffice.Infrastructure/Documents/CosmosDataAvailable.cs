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

namespace Energinet.DataHub.PostOffice.Infrastructure.Documents
{
    public class CosmosDataAvailable
    {
        public CosmosDataAvailable()
        {
            Id = Guid.NewGuid().ToString();
            Uuid = null!;
            ContentType = null!;
            Origin = null!;
            Recipient = null!;
            _ts = null!;
        }

        public string Id { get; set; }

        public string Uuid { get; set; }

        public string ContentType { get; set; }

        public string Origin { get; set; }

        public string Recipient { get; set; }

        public bool SupportsBundling { get; set; }

        public int RelativeWeight { get; set; }

        public bool Acknowledge { get; set; }

#pragma warning disable CA1707, SA1300
        public string _ts { get; set; }
#pragma warning restore
    }
}
