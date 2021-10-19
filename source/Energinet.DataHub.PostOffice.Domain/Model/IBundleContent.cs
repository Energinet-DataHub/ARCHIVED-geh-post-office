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

using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Domain.Model.Logging;

namespace Energinet.DataHub.PostOffice.Domain.Model
{
    /// <summary>
    /// Represents data for a specific bundle.
    /// </summary>
    public interface IBundleContent : IProviderLogIdentifier
    {
        /// <summary>
        /// Opens a stream to the content contained within the bundle.
        /// It is the responsibility of the caller to close the stream after use.
        /// </summary>
        /// <returns>A stream to the content contained within the bundle.</returns>
        Task<Stream> OpenAsync();
    }
}
