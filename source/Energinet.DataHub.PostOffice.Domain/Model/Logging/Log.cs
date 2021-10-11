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

namespace Energinet.DataHub.PostOffice.Domain.Model.Logging
{
    public sealed class Log
    {
        public Log(
            string endpointType,
            GlobalLocationNumber gln,
            string processId,
            IBundleContent? bundleReference = null)
        {
            EndpointType = endpointType;
            MarketOperator = gln;
            ProcessId = processId;
            BundleReference = bundleReference;
        }

        public string Id { get; } = Guid.NewGuid().ToString();

        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string EndpointType { get; }
        public GlobalLocationNumber MarketOperator { get; }
        public string ProcessId { get; }
        public IBundleContent? BundleReference { get; }
    }
}
