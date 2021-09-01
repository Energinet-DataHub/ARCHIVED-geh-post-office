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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.EntryPoint.MarketOperator.Functions;
using Energinet.DataHub.PostOffice.Tests.Common;
using MediatR;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.MarketOperator
{
    [UnitTest]
    public sealed class PeekAzureFunctionTests
    {
        private readonly Uri _functionRoute = new("https://localhost?recipient=0101010101010");

        [Fact]
        public async Task Run_HasData_ReturnsStatusOkWithStream()
        {
            // Arrange
            const string expectedData = "expected_data";

            var mockedMediator = new Mock<IMediator>();
            var mockedFunctionContext = new MockedFunctionContext();

            var mockedRequestData = new Mock<HttpRequestData>(mockedFunctionContext.FunctionContext);
            mockedRequestData.Setup(x => x.Url).Returns(_functionRoute);

            mockedMediator
                .Setup(x => x.Send(It.IsAny<PeekCommand>(), default))
                .ReturnsAsync(new PeekResponse(true, new MemoryStream(Encoding.ASCII.GetBytes(expectedData))));

            var target = new Peek(mockedMediator.Object);

            // Act
            var response = await target.RunAsync(mockedRequestData.Object, mockedFunctionContext).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedData, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        [Fact]
        public async Task Run_HasNoData_ReturnsStatusNoContent()
        {
            // Arrange
            var mockedMediator = new Mock<IMediator>();
            var mockedFunctionContext = new MockedFunctionContext();

            var mockedRequestData = new Mock<HttpRequestData>(mockedFunctionContext.FunctionContext);
            mockedRequestData.Setup(x => x.Url).Returns(_functionRoute);

            mockedMediator
                .Setup(x => x.Send(It.IsAny<PeekCommand>(), default))
                .ReturnsAsync(new PeekResponse(false, Stream.Null));

            var target = new Peek(mockedMediator.Object);

            // Act
            var response = await target.RunAsync(mockedRequestData.Object, mockedFunctionContext).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        //[Fact]
        //public void Run_InvalidInput_IsHandled()
        //{
        //    // TODO: Error handling not defined.
        //}

        //[Fact]
        //public void Run_HandlerException_IsHandled()
        //{
        //    // TODO: Error handling not defined.
        //}
    }
}
