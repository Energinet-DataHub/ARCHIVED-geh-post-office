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

namespace Energinet.DataHub.PostOffice.Common.Configuration;

public static class Settings
{
    public const string IntegrationEventConnectionStringKey = "INTEGRATION_EVENT_CONNECTION_STRING";
    public const string MarketParticipantActorUpdatedSubscriptionNameKey = "MARKET_PARTICIPANT_ACTOR_UPDATED_SUBSCRIPTION_NAME";
    public const string IntegrationEventTopicNameKey = "INTEGRATION_EVENT_TOPIC_NAME";

    public static Setting<string> MessagesDbConnectionString { get; }
        = new("MESSAGES_DB_CONNECTION_STRING");
    public static Setting<string> MessagesDbId { get; }
        = new("MESSAGES_DB_NAME");

    public static Setting<string> BlobStorageConnectionString { get; }
        = new("BlobStorageConnectionString");
    public static Setting<string> BlobStorageContainerName { get; }
        = new("BlobStorageContainerName");

    public static Setting<string> SqlActorDbConnectionString { get; }
        = new("SQL_ACTOR_DB_CONNECTION_STRING");
    public static Setting<string> IntegrationEventConnectionString { get; }
        = new(IntegrationEventConnectionStringKey);
    public static Setting<string> IntegrationEventTopicName { get; }
        = new(IntegrationEventTopicNameKey);
    public static Setting<string> MarketParticipantActorUpdatedSubscriptionName { get; }
        = new(MarketParticipantActorUpdatedSubscriptionNameKey);

    public static Setting<string> DataAvailableConnectionString { get; }
        = new("DATAAVAILABLE_QUEUE_CONNECTION_STRING");
    public static Setting<string> DataAvailableQueueName { get; }
        = new("DATAAVAILABLE_QUEUE_NAME");
    public static Setting<int> DataAvailableBatchSize { get; }
        = new("DATAAVAILABLE_BATCH_SIZE", 10000);
    public static Setting<int> DataAvailableTimeoutMs { get; }
        = new("DATAAVAILABLE_TIMEOUT_IN_MS", 1000);

    public static Setting<string> TimeSeriesQueue { get; }
        = new("TIMESERIES_QUEUE_NAME", "timeseries");
    public static Setting<string> TimeSeriesReplyQueue { get; }
        = new("TIMESERIES_REPLY_QUEUE_NAME", "timeseries-reply");
    public static Setting<string> ChargesQueue { get; }
        = new("CHARGES_QUEUE_NAME", "charges");
    public static Setting<string> ChargesReplyQueue { get; }
        = new("CHARGES_REPLY_QUEUE_NAME", "charges-reply");
    public static Setting<string> MarketRolesQueue { get; }
        = new("MARKETROLES_QUEUE_NAME", "marketroles");
    public static Setting<string> MarketRolesReplyQueue { get; }
        = new("MARKETROLES_REPLY_QUEUE_NAME", "marketroles-reply");
    public static Setting<string> MeteringPointsQueue { get; }
        = new("METERINGPOINTS_QUEUE_NAME", "meteringpoints");
    public static Setting<string> MeteringPointsReplyQueue { get; }
        = new("METERINGPOINTS_REPLY_QUEUE_NAME", "meteringpoints-reply");
    public static Setting<string> WholesaleQueue { get; }
        = new("WHOLESALE_QUEUE_NAME", "wholesale");
    public static Setting<string> WholesaleReplyQueue { get; }
        = new("WHOLESALE_REPLY_QUEUE_NAME", "wholesale-reply");

    public static Setting<string> TimeSeriesDequeueQueue { get; }
        = new("TIMESERIES_DEQUEUE_QUEUE_NAME", "timeseries-dequeue");
    public static Setting<string> ChargesDequeueQueue { get; }
        = new("CHARGES_DEQUEUE_QUEUE_NAME", "charges-dequeue");
    public static Setting<string> MarketRolesDequeueQueue { get; }
        = new("MARKETROLES_DEQUEUE_QUEUE_NAME", "marketroles-dequeue");
    public static Setting<string> MeteringPointsDequeueQueue { get; }
        = new("METERINGPOINTS_DEQUEUE_QUEUE_NAME", "meteringpoints-dequeue");
    public static Setting<string> WholesaleDequeueQueue { get; }
        = new("WHOLESALE_DEQUEUE_QUEUE_NAME", "wholesale-dequeue");

    public static Setting<string> OpenIdTenantId { get; }
        = new("B2C_TENANT_ID");
    public static Setting<string> OpenIdAudience { get; }
        = new("BACKEND_SERVICE_APP_ID");

    public static Setting<string> RequestResponseLogConnectionString { get; }
        = new("RequestResponseLogConnectionString");
    public static Setting<string> RequestResponseLogContainerName { get; }
        = new("RequestResponseLogContainerName");

    public static Setting<string> AppInsightsInstrumentationKey { get; }
        = new("APPINSIGHTS_INSTRUMENTATIONKEY");
    public static Setting<string> ServiceBusHealthCheckConnectionString { get; }
        = new("SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING");
}
