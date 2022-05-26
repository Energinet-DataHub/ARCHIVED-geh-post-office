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

using System.Threading.Tasks;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Common.Configuration;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.Operations.Functions
{
    public sealed class MarketParticipantIngestionFunction
    {
        private const string FunctionName = "MarketParticipantIngestion";

        private readonly ILogger<MarketParticipantIngestionFunction> _logger;
        private readonly IMediator _mediator;
        private readonly ISharedIntegrationEventParser _sharedIntegrationEventParser;

        public MarketParticipantIngestionFunction(
            ILogger<MarketParticipantIngestionFunction> logger,
            IMediator mediator,
            ISharedIntegrationEventParser sharedIntegrationEventParser)
        {
            _logger = logger;
            _mediator = mediator;
            _sharedIntegrationEventParser = sharedIntegrationEventParser;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + Settings.MarketParticipantTopicNameKey + "%",
                "%" + Settings.MarketParticipantSubscriptionNameKey + "%",
                Connection = Settings.MarketParticipantConnectionStringKey)]
            byte[] message)
        {
            _logger.LogInformation("Begins processing MarketParticipantSyncFunction.");

            var integrationEvent = _sharedIntegrationEventParser.Parse(message);
            if (integrationEvent is ActorUpdatedIntegrationEvent actorUpdated)
            {
                if (actorUpdated.Status is ActorStatus.Active or ActorStatus.Passive &&
                    actorUpdated.ExternalActorId.HasValue)
                {
                    var command = new UpdateActorCommand(
                        actorUpdated.ActorId,
                        actorUpdated.ExternalActorId.Value);

                    await _mediator.Send(command).ConfigureAwait(false);
                }
                else
                {
                    var command = new DeleteActorCommand(actorUpdated.ActorId);
                    await _mediator.Send(command).ConfigureAwait(false);
                }
            }

            _logger.LogInformation("Success processing MarketParticipantSyncFunction.");
        }
    }
}
