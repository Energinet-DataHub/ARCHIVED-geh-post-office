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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Client.Extensions;
using Energinet.DataHub.MessageHub.Model.IntegrationEvents;
using NodaTime.Text;
using Xunit;

namespace Energinet.DataHub.MessageHub.Client.Tests;

public class ServiceBusMessageExtensionsTests
{
    [Fact]
    public void AddDataAvailableIntegrationEvents_WithValidData_CanBeReadBack()
    {
        // Arrange
        var target = new ServiceBusMessage();
        var correlationId = Guid.NewGuid().ToString();

        // Act
        target.AddDataAvailableIntegrationEvents(correlationId);

        // Assert
        var properties = target.ApplicationProperties;

        AssertValidTimestamp(properties);
        Assert.Equal(correlationId, properties["OperationCorrelationId"]);
        Assert.Equal(1, properties["MessageVersion"]);
        Assert.Equal(nameof(IntegrationEventsMessageType.DataAvailable), properties["MessageType"]);
        Assert.True(Guid.TryParse((string)properties["EventIdentification"], out _));
    }

    [Fact]
    public void AddDataBundleResponseIntegrationEvents_WithValidData_CanBeReadBack()
    {
        // Arrange
        var target = new ServiceBusMessage();
        var correlationId = Guid.NewGuid().ToString();

        // Act
        target.AddDataBundleResponseIntegrationEvents(correlationId);

        // Assert
        var properties = target.ApplicationProperties;

        AssertValidTimestamp(properties);
        Assert.Equal(correlationId, properties["OperationCorrelationId"]);
        Assert.Equal(1, properties["MessageVersion"]);
        Assert.Equal(nameof(IntegrationEventsMessageType.DataBundleResponse), properties["MessageType"]);
        Assert.True(Guid.TryParse((string)properties["EventIdentification"], out _));
    }

    private static void AssertValidTimestamp(IDictionary<string, object> properties)
    {
        var timestamp = (string)properties["OperationTimestamp"];
        var instant = InstantPattern.ExtendedIso.Parse(timestamp);

        Assert.True(instant.Success);
        Assert.Equal(instant.Value.ToString(), timestamp);
    }
}
