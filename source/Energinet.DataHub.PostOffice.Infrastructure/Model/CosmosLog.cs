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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Model.Logging;

namespace Energinet.DataHub.PostOffice.Infrastructure.Model
{
    public sealed class CosmosLog
    {
        public CosmosLog(
            string id,
            string endpointType,
            string globalLocationNumber,
            string processId,
            string? bundleReference = null)
        {
            Id = id;
            EndpointType = endpointType;
            GlobalLocationNumber = globalLocationNumber;
            ProcessId = processId;
            BundleReference = bundleReference;
        }

        public string Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string EndpointType { get; }
        public string GlobalLocationNumber { get; }
        public string ProcessId { get; }
        public string? BundleReference { get; set; }
    }
}
