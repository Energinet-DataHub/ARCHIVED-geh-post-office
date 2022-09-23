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
using System.Linq;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions.Helpers
{
    public sealed class ExternalResponseFormatProvider : IExternalResponseFormatProvider
    {
        /// <summary>
        ///     Get the bundle id from the request, or returns null if no bundle id was provided.
        /// </summary>
        /// <param name="request">The request to probe for the bundle id.</param>
        /// <returns>The bundle id, or null.</returns>
        public ResponseFormat TryGetResponseFormat(HttpRequestData request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var maybeResponseFormat = request.Url.GetQueryValue(Constants.ResponseFormatQueryName);
            if (!string.IsNullOrEmpty(maybeResponseFormat) && Enum.TryParse<ResponseFormat>(maybeResponseFormat, true, out var format))
            {
                return Enum.IsDefined(format) ? format : ResponseFormat.Xml;
            }

            if (!request.Headers.TryGetValues("accept", out var values)) return ResponseFormat.Xml;
            return values.Contains("application/json") ? ResponseFormat.Json : ResponseFormat.Xml;
        }
    }
}
