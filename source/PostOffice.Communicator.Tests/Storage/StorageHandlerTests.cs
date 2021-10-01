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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using GreenEnergyHub.PostOffice.Communicator.Exceptions;
using GreenEnergyHub.PostOffice.Communicator.Factories;
using GreenEnergyHub.PostOffice.Communicator.Model;
using GreenEnergyHub.PostOffice.Communicator.Storage;
using Moq;
using Xunit;
using Xunit.Categories;
using static System.Guid;

namespace PostOffice.Communicator.Tests.Storage
{
    [UnitTest]
    public class StorageHandlerTests
    {
        [Fact]
        public async Task AddStreamToStorageAsync_StreamIsEmpty_ThrowsArgumentException()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();
            var mockedDataBundleRequestDto = new DataBundleRequestDto(
                NewGuid().ToString(),
                new List<Guid>() { NewGuid(), NewGuid(), NewGuid() });

            mockedStorageServiceClientFactory.Setup(
                    x => x.Create())
                .Returns(mockedBlobServiceClient.Object);

            var target = new StorageHandler(mockedStorageServiceClientFactory.Object);

            // act, assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => target.AddStreamToStorageAsync(
                    Stream.Null,
                    mockedDataBundleRequestDto))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task AddStreamToStorageAsync_RequestFailure_ThrowsCustomException()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();
            var mockedBlobContainerClient = new Mock<BlobContainerClient>();
            var mockedDataBundleRequestDto = new DataBundleRequestDto(
                NewGuid().ToString(),
                new List<Guid>() { NewGuid(), NewGuid(), NewGuid() });

            mockedBlobServiceClient.Setup(
                    x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockedBlobContainerClient.Object);

            mockedBlobContainerClient.Setup(
                    x => x.UploadBlobAsync(
                        It.IsAny<string>(),
                        It.IsAny<Stream>(),
                        default))
                .ThrowsAsync(new RequestFailedException("test"));

            mockedStorageServiceClientFactory.Setup(
                    x => x.Create())
                .Returns(mockedBlobServiceClient.Object);

            var target = new StorageHandler(mockedStorageServiceClientFactory.Object);

            // act, assert
            await using var inputStream = new MemoryStream(new byte[] { 1, 2, 3 });
            await Assert.ThrowsAsync<PostOfficeCommunicatorStorageException>(
                    () => target.AddStreamToStorageAsync(
                        inputStream,
                        mockedDataBundleRequestDto))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task AddStreamToStorageAsync_StreamIsValid_ReturnsUri()
        {
            // arrange
            var mockedStorageServiceClientFactory = new Mock<IStorageServiceClientFactory>();
            var mockedBlobServiceClient = new Mock<BlobServiceClient>();
            var mockedBlobContainerClient = new Mock<BlobContainerClient>();
            var mockedBlobClient = new Mock<BlobClient>();
            var mockedDataBundleRequestDto = new DataBundleRequestDto(
                NewGuid().ToString(),
                new List<Guid>() { NewGuid(), NewGuid(), NewGuid() });

            var testUri = new Uri($"http://test.test.dk/FileStorage/{DomainOrigin.TimeSeries}-postoffice-blobstorage");
            mockedBlobClient.Setup(
                    x => x.Uri)
                .Returns(testUri);

            mockedBlobContainerClient.Setup(
                x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>(), default));

            mockedBlobContainerClient.Setup(
                    x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(mockedBlobClient.Object);

            mockedBlobServiceClient.Setup(
                    x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockedBlobContainerClient.Object);

            mockedStorageServiceClientFactory.Setup(
                    x => x.Create())
                .Returns(mockedBlobServiceClient.Object);

            var target = new StorageHandler(mockedStorageServiceClientFactory.Object);

            // act
            await using var inputStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var result = await target.AddStreamToStorageAsync(inputStream, mockedDataBundleRequestDto).ConfigureAwait(false);

            // assert
            Assert.Equal(testUri, result);
        }
    }
}
