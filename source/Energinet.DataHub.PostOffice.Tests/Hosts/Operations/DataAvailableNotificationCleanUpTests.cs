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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.GridArea;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Organization;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.EntryPoint.Operations.Functions;
using MediatR;
using Microsoft.Azure.Functions.Isolated.TestDoubles;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Hosts.Operations
{
    [UnitTest]
    public sealed class DataAvailableNotificationCleanUpTests
    {
        [Fact]
        public async Task RunAsync_FunctionRun_And_MediatorIsCalled()
        {
            // Arrange
            var logger = new Mock<ILogger<DataAvailableNotificationCleanUpFunction>>().Object;
            var mediator = new Mock<IMediator>();

            var target = new DataAvailableNotificationCleanUpFunction(logger, mediator.Object);

            // Act
            using var functionContext = new MockFunctionContext();
            await target.RunAsync(functionContext).ConfigureAwait(false);

            // Assert
            mediator.Verify(m => m.Send(It.IsAny<DataAvailableNotificationCleanUpCommand>(), CancellationToken.None), Times.Once);
        }
    }
}
