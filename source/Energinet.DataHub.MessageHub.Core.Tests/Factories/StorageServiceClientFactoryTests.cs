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

using Energinet.DataHub.MessageHub.Core.Factories;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Core.Tests.Factories
{
    [UnitTest]
    public sealed class StorageServiceClientFactoryTests
    {
        [Fact]
        public void Create_ReturnsBlobServiceClient()
        {
            // arrange
            var target = new StorageServiceClientFactory("DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;EndpointSuffix=core.windows.net");

            // act
            var actual = target.Create();

            // assert
            Assert.NotNull(actual);
        }
    }
}
