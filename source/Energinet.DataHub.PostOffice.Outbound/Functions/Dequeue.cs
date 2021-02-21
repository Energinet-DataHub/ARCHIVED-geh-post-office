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
using System.Threading.Tasks;
using System.Web.Http;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Outbound.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public class Dequeue
    {
        private readonly IDocumentStore _documentStore;

        public Dequeue(
            IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        [FunctionName("Dequeue")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest request,
            ILogger logger)
        {
            var documentBody = await request.GetDocumentBody();
            if (string.IsNullOrEmpty(documentBody.Recipient)) return new BadRequestErrorMessageResult("Query parameter is missing 'recipient'");
            if (string.IsNullOrEmpty(documentBody.Bundle)) return new BadRequestErrorMessageResult("Query parameter is missing 'type'");

            var bundle = request.Query["id"].ToString();
            var recipient = request.Query["recipient"].ToString();

            logger.LogInformation("processing document dequeue: {id}", bundle);

            var didDeleteDocuments = await _documentStore
                .DeleteDocumentsAsync(documentBody)
                .ConfigureAwait(false);

            return didDeleteDocuments
                ? (IActionResult)new OkResult()
                : (IActionResult)new NotFoundResult();
        }
    }
}
