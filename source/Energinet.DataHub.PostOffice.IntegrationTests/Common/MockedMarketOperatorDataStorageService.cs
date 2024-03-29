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
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Services;
using Microsoft.AspNetCore.WebUtilities;

namespace Energinet.DataHub.PostOffice.IntegrationTests.Common
{
    internal sealed class MockedMarketOperatorDataStorageService : IMarketOperatorDataStorageService
    {
        public Task<Stream> GetMarketOperatorDataAsync(Uri contentPath)
        {
            // Integration testing: the Protobuf request is sent raw in the Url.
            var query = QueryHelpers.ParseQuery(contentPath.Query);
            var message = query["mocked"];
            var bytes = Convert.FromBase64String(message);
            return Task.FromResult<Stream>(new MemoryStream(bytes));
        }
    }
}
