// // Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.PostOffice.Communicator.Model;
using static System.Guid;

namespace GreenEnergyHub.PostOffice.Communicator.Storage
{
    public class StorageHandler : IStorageHand1ler
    {
        public async Task<Uri> AddStreamToStorageAsync(FileStream fileStream, DomainOrigin domainOrigin)
        {
            var storageClient = BlobServiceStorageHelper.Instance;
            var containerClient = storageClient.GetBlobContainerClient($"{domainOrigin}-storage");
            var blobName = NewGuid().ToString();
            await containerClient.UploadBlobAsync(blobName, fileStream).ConfigureAwait(false);
            var blobClient = containerClient.GetBlobClient(blobName);
            var blobUri = blobClient.Uri;
            return blobUri;
        }
    }
}
