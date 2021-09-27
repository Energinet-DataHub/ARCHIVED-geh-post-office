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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;

namespace Energinet.DataHub.PostOffice.Infrastructure.Model
{
    public sealed class AzureBlobBundleContent : IBundleContent
    {
        private readonly Uuid _bundleId;
        private readonly IMarketOperatorDataStorageService _marketOperatorDataStorageService;

        public AzureBlobBundleContent(
            IMarketOperatorDataStorageService marketOperatorDataStorageService,
            Uuid bundleId,
            Uri contentPath)
        {
            _marketOperatorDataStorageService = marketOperatorDataStorageService;
            _bundleId = bundleId;
            ContentPath = contentPath;
        }

        public Uri ContentPath { get; }

        public Task<Stream> OpenAsync()
        {
            return _marketOperatorDataStorageService.GetMarketOperatorDataAsync(_bundleId, ContentPath);
        }
    }
}