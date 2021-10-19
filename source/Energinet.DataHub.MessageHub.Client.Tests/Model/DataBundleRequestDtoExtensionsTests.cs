using System;
using System.Collections.Generic;
using Energinet.DataHub.MessageHub.Client.Extensions;
using Energinet.DataHub.MessageHub.Client.Model;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Client.Tests.Model
{
    [UnitTest]
    public class DataBundleRequestDtoExtensionsTests
    {
        [Fact]
        public void CreateResponse_RequestNull_Throws()
        {
            // arrange, act, assert
            Assert.Throws<ArgumentNullException>(() => ((DataBundleRequestDto)null)!.CreateResponse(new Uri("http://localhost")));
        }

        [Fact]
        public void CreateResponse_ReturnsResponse()
        {
            // arrage
            var dataAvailableNotificationIds = new List<Guid> { Guid.NewGuid() };
            var request = new DataBundleRequestDto("some_value", dataAvailableNotificationIds);
            var uri = new Uri("http://localhost");

            // act
            var actual = request.CreateResponse(uri);

            // assert
            Assert.Equal(uri, actual.ContentUri);
            Assert.Equal(dataAvailableNotificationIds, actual.DataAvailableNotificationIds);
        }
    }
}
