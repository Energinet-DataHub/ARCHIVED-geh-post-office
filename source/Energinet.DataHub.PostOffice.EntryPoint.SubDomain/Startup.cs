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
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Common;
using Energinet.DataHub.PostOffice.EntryPoint.SubDomain.Functions;
using Energinet.DataHub.PostOffice.Infrastructure;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.EntryPoint.SubDomain
{
    internal sealed class Startup : StartupBase
    {
        protected override void Configure(Container container)
        {
            container.RegisterSingleton<IDataAvailableMessageReceiver>(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var batchSize = configuration.GetValue<int>("DATAAVAILABLE_BATCH_SIZE");
                var timeoutInMs = configuration.GetValue<int>("DATAAVAILABLE_TIMEOUT_IN_MS");

                var serviceBusConfig = container.GetInstance<ServiceBusConfig>();
                var messageReceiver = new MessageReceiver(
                    serviceBusConfig.DataAvailableQueueConnectionString,
                    serviceBusConfig.DataAvailableQueueName,
                    prefetchCount: batchSize);

                return new DataAvailableMessageReceiver(messageReceiver, batchSize, TimeSpan.FromMilliseconds(timeoutInMs));
            });

            container.Register<IMapper<DataAvailableNotificationDto, DataAvailableNotificationCommand>, DataAvailableMapper>(Lifestyle.Scoped);
            container.Register<DataAvailableInbox>(Lifestyle.Scoped);
            container.Register<DataAvailableTimerTrigger>(Lifestyle.Scoped);
        }
    }
}
