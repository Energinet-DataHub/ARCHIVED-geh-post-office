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
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Utilities
{
    public static class LoggerExtensions
    {
        public static void LogProcess(this ILogger source, string entryPoint, string correlationId, string gln)
        {
            source.LogInformation("EntryPoint={EntryPoint};CorrelationId={CorrelationId};Gln={Gln}", entryPoint, correlationId, gln);
        }

        public static void LogProcess(this ILogger source, string entryPoint, string status, string correlationId, string gln, string bundleId)
        {
            source.LogInformation("EntryPoint={EntryPoint};Status={Status};CorrelationId={CorrelationId};Gln={Gln};BundleId={BundleId}", entryPoint, status, correlationId, gln, bundleId);
        }

        public static void LogProcess(this ILogger source, string entryPoint, string status, string correlationId, string gln, string bundleId, string domain)
        {
            source.LogInformation("EntryPoint={EntryPoint};Status={Status};CorrelationId={CorrelationId};Gln={Gln};BundleId={BundleId};Domain={Domain}", entryPoint, status, correlationId, gln, bundleId, domain);
        }

        public static void LogProcess(this ILogger source, string entryPoint, string status, string correlationId, string gln, string bundleId, IEnumerable<string> dataAvailables)
        {
            source.LogInformation("EntryPoint={EntryPoint};Status={Status};CorrelationId={CorrelationId};Gln={Gln};BundleId={BundleId};DataAvailables={DataAvailables}", entryPoint, status, correlationId, gln, bundleId, string.Join(",", dataAvailables));
        }
    }
}
