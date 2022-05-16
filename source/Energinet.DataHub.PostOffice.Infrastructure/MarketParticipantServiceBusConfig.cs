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

namespace Energinet.DataHub.PostOffice.Infrastructure;

public sealed class MarketParticipantServiceBusConfig
{
    public const string MarketParticipantConnectionStringKey = "MARKET_PARTICIPANT_CONNECTION_STRING";
    public const string MarketParticipantTopicNameKey = "MARKET_PARTICIPANT_TOPIC_NAME";
    public const string MarketParticipantSubscriptionNameKey = "MARKET_PARTICIPANT_SUBSCRIPTION_NAME";

    public MarketParticipantServiceBusConfig(
        string marketParticipantConnectionString,
        string marketParticipantTopicName,
        string marketParticipantSubscriptionName)
    {
        if (string.IsNullOrWhiteSpace(marketParticipantConnectionString))
            throw new InvalidOperationException($"{MarketParticipantConnectionStringKey} must be specified in {nameof(DataAvailableServiceBusConfig)}");

        if (string.IsNullOrWhiteSpace(marketParticipantTopicName))
            throw new InvalidOperationException($"{MarketParticipantTopicNameKey} must be specified in {nameof(DataAvailableServiceBusConfig)}");

        if (string.IsNullOrWhiteSpace(marketParticipantSubscriptionName))
            throw new InvalidOperationException($"{MarketParticipantSubscriptionNameKey} must be specified in {nameof(DataAvailableServiceBusConfig)}");

        MarketParticipantConnectionString = marketParticipantConnectionString;
        MarketParticipantTopicName = marketParticipantTopicName;
        MarketParticipantSubscriptionName = marketParticipantSubscriptionName;
    }

    public string MarketParticipantConnectionString { get; }
    public string MarketParticipantTopicName { get; }
    public string MarketParticipantSubscriptionName { get; }
}
