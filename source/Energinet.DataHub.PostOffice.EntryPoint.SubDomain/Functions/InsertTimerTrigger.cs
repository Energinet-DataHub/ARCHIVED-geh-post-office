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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.Commands;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions
{
    public class InsertTimerTrigger
    {
        private readonly IMediator _mediator;
        private readonly IDataAvailableMessageReceiver _messageReceiver;
        private readonly IDataAvailableNotificationParser _dataAvailableNotificationParser;
        private readonly IMapper<DataAvailableDto, DataAvailablesForRecipientCommand> _dataAvailableNotificationMapper;

        public InsertTimerTrigger(
            IMediator mediator,
            IDataAvailableMessageReceiver messageReceiver,
            IDataAvailableNotificationParser dataAvailableNotificationParser,
            IMapper<DataAvailableDto, DataAvailablesForRecipientCommand> dataAvailableNotificationMapper)
        {
            _mediator = mediator;
            _messageReceiver = messageReceiver;
            _dataAvailableNotificationParser = dataAvailableNotificationParser;
            _dataAvailableNotificationMapper = dataAvailableNotificationMapper;
        }

        [Function("InsertTimerTrigger")]
        public async Task RunAsync([TimerTrigger("0 */1 * * * *")] FunctionContext context)
        {
            var logger = context.GetLogger("InsertTimerTrigger");
            logger.LogInformation("Begins processing InsertTimerTrigger.");

            try
            {
                var messages = await _messageReceiver.ReceiveAsync().ConfigureAwait(false);

                var notifications = messages.Select(x => _dataAvailableNotificationParser.TryParse(x, x.MessageId));

                var notificationsPrRecipient = notifications
                    .Where(x => x.CouldParse)
                    .GroupBy(x => x.Recipient);

                var results = new List<DataAvailableNotificationResponse>();
                var parsedDataAvailableList = new List<DataAvailableDto>();

                Parallel.ForEach(notificationsPrRecipient, x =>
                {
                    foreach (var da in x)
                    {
                        var dataAvailableCommand = _dataAvailableNotificationMapper.Map(da);
                        var response = _mediator.Send(dataAvailableCommand);
                        results.Add(response.Result);
                        parsedDataAvailableList.Add(da);
                    }
                });

                // Error handling here before completing and deadlettering
                var completeMessages = messages.Where(x => parsedDataAvailableList.Any(y => y.Uuid.ToString() == x.MessageId)).ToList();
                var deadletterMessages = messages.Except(completeMessages).ToList();

                await _messageReceiver.CompleteAsync(completeMessages).ConfigureAwait(false);
                await _messageReceiver.DeadLetterAsync(deadletterMessages).ConfigureAwait(false);

                await _mediator.Send(new UpdateMaximumSequenceNumberCommand(0000)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
