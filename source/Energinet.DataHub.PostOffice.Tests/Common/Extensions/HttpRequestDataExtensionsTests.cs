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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.Extensions
{
    [UnitTest]
    public class HttpRequestDataExtensionsTests
    {
        [Fact]
        public void CreateResponse_ValidStream_ReturnsResponse()
        {
            // arrange
            using var stream = new MemoryStream();
            var response = new Mock<MockableHttpResponseData>();
            var request = new Mock<MockableHttpRequestData>();
            request.Setup(x => x.CreateResponse())
                .Returns(response.Object);

            // act
            request.Object.CreateResponse(stream);

            // assert
            // ReSharper disable once AccessToDisposedClosure
            response.VerifySet(x => x.Body = stream, Times.Once());
            response.VerifySet(x => x.StatusCode = HttpStatusCode.OK, Times.Once());
        }

        [Fact]
        public void CreateResponse_SourceIsNull_ThrowsException()
        {
            // arrange
            var request = (HttpRequestData)null!;

            // act, assert
            Assert.Throws<ArgumentNullException>(() => request.CreateResponse(new MemoryStream()));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        internal abstract class MockableHttpRequestData : HttpRequestData
        {
#pragma warning disable 8618
            protected MockableHttpRequestData()
#pragma warning restore 8618
                : base(new FakeFunctionContext())
            {
            }

            public override Stream Body => null!;
            public override HttpHeadersCollection Headers => null!;
            public override IReadOnlyCollection<IHttpCookie> Cookies => null!;
            public override Uri Url => null!;
            public override IEnumerable<ClaimsIdentity> Identities => null!;
            public override string Method => null!;

            public override HttpResponseData CreateResponse()
            {
                throw new NotSupportedException();
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        internal abstract class MockableHttpResponseData : HttpResponseData
        {
#pragma warning disable 8618
            protected MockableHttpResponseData()
#pragma warning restore 8618
                : base(new FakeFunctionContext())
            {
            }

            public override HttpStatusCode StatusCode { get; set; }
            public override HttpHeadersCollection Headers { get; set; }
            public override Stream Body { get; set; }

            // ReSharper disable once UnassignedGetOnlyAutoProperty
            public override HttpCookies Cookies { get; }
        }

        private sealed class FakeFunctionContext : FunctionContext
        {
#pragma warning disable 8618
            public override string InvocationId => null!;
            public override string FunctionId => null!;
            public override TraceContext TraceContext => null!;
            public override BindingContext BindingContext => null!;
            public override IServiceProvider InstanceServices { get; set; } = null!;
            public override FunctionDefinition FunctionDefinition => null!;
            public override IDictionary<object, object> Items { get; set; } = null!;
            public override IInvocationFeatures Features => null!;
#pragma warning restore 8618
        }
    }
}
