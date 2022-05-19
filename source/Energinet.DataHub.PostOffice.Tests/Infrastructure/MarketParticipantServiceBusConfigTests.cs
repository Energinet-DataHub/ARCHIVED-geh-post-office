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
using Energinet.DataHub.PostOffice.Infrastructure;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Infrastructure;

[UnitTest]
public sealed class MarketParticipantServiceBusConfigTests
{
    [Fact]
    public void Ctor_ParamsNotNull_SetsProperties()
    {
        // arrange
        const string connectionString = "connectionString";
        const string topicName = "topicName";
        const string subscriptionName = "subscriptionName";

        // act
        var actual = new MarketParticipantServiceBusConfig(
             connectionString,
             topicName,
             subscriptionName);

        // assert
        Assert.Equal(connectionString, actual.MarketParticipantConnectionString);
        Assert.Equal(topicName, actual.MarketParticipantTopicName);
        Assert.Equal(subscriptionName, actual.MarketParticipantSubscriptionName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    public void Ctor_TopicNameNullOrWhitespace_Throws(string value)
    {
        // arrange, act, assert
        Assert.Throws<InvalidOperationException>(() => new MarketParticipantServiceBusConfig("a", value, "b"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    public void Ctor_ConnectionStringNullOrWhitespace_Throws(string value)
    {
        // arrange, act, assert
        Assert.Throws<InvalidOperationException>(() => new MarketParticipantServiceBusConfig(value, "a", "b"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    public void Ctor_SubscriptionNullOrWhitespace_Throws(string value)
    {
        // arrange, act, assert
        Assert.Throws<InvalidOperationException>(() => new MarketParticipantServiceBusConfig("a", "b", value));
    }
}
