﻿// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.Model;

namespace Energinet.DataHub.MessageHub.Client.Storage
{
    /// <summary>
    /// Handles storing file data from the SubDomains
    /// </summary>
    public interface IStorageHandler
    {
        /// <summary>
        /// Stores a filestream in the PostOffice storage, and returns a path to the stored file,
        /// that is to be used when sending a response to the PostOffice
        /// </summary>
        /// <param name="stream">A stream containing the contents that should be stored</param>
        /// <param name="requestDto">THe domain that is sending the data</param>
        /// <returns>A string containing the path of the stored file</returns>
        Task<Uri> AddStreamToStorageAsync(Stream stream, DataBundleRequestDto requestDto);

        /// <summary>
        /// Retrieves a stream from the storage
        /// </summary>
        /// <param name="contentPath">The uri to the content in storage</param>
        /// <returns>A Stream to the contents in storage</returns>
        Task<Stream> GetStreamFromStorageAsync(Uri contentPath);
    }
}