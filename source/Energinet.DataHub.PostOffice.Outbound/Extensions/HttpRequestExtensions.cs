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
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Energinet.DataHub.PostOffice.Outbound.Extensions
{
    public static class HttpRequestExtensions
    {
        public static DocumentQuery GetDocumentQuery(this HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var type = request.Query.ContainsKey("type") ? request.Query["type"].ToString() : null;
            var recipient = request.Query.ContainsKey("recipient") ? request.Query["recipient"].ToString() : null;
            if (type == null || recipient == null)
            {
                throw new InvalidOperationException("Request must include type and recipient.");
            }

            var documentQuery = new DocumentQuery(recipient!, type!);

            if (request.Query.ContainsKey("pageSize") && int.TryParse(request.Query["pageSize"], out var pageSize))
            {
                documentQuery.PageSize = pageSize;
            }

            return documentQuery;
        }

        public static async Task<DocumentBody> GetDocumentBody(this HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();

            // use Json.NET to deserialize the posted JSON into a C# dynamic object
            var parsedObject = JsonConvert.DeserializeObject<DocumentBody>(requestBody);

            if (parsedObject == null)
            {
                throw new InvalidOperationException("Object could not be parsed");
            }

            return parsedObject;
        }
    }
}
