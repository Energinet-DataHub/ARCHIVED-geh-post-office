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
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Services;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Services.Domain
{
    [UnitTest]
    public class ContentTypeMaxWeightMapTests
    {
        [Theory]
        [InlineData(ContentType.Unknown)]
        [InlineData(ContentType.TimeSeries)]
        public void Map_ContentType_ReturnsWeight(ContentType contentType)
        {
            // arrange
            var target = new ContentTypeMaxWeightMap();

            // act
            var actual = target.Map(contentType);

            // assert
            Assert.Equal(new Weight(1), actual);
        }

        [Fact]
        public void Map_ContentTypeUndefined_ThrowsException()
        {
            // arrange
            var target = new ContentTypeMaxWeightMap();

            // act, assert
            Assert.Throws<ArgumentOutOfRangeException>(() => target.Map((ContentType)(-1)));
        }
    }
}
