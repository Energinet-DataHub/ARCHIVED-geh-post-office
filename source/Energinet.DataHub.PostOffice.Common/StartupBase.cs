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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.PostOffice.Application;
using Energinet.DataHub.PostOffice.Application.DataAvailable;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;
using Energinet.DataHub.PostOffice.Application.Validation;
using Energinet.DataHub.PostOffice.Common.MediatR;
using Energinet.DataHub.PostOffice.Common.SimpleInjector;
using Energinet.DataHub.PostOffice.Contracts;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using Energinet.DataHub.PostOffice.Infrastructure;
using Energinet.DataHub.PostOffice.Infrastructure.ContentPath;
using Energinet.DataHub.PostOffice.Infrastructure.GetMessage;
using Energinet.DataHub.PostOffice.Infrastructure.Mappers;
using Energinet.DataHub.PostOffice.Infrastructure.MessageReplyStorage;
using Energinet.DataHub.PostOffice.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common
{
    public abstract class StartupBase : IAsyncDisposable
    {
        protected StartupBase()
        {
            Container = new Container();
        }

        public Container Container { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var descriptor = new ServiceDescriptor(
                typeof(IFunctionActivator),
                typeof(SimpleInjectorActivator),
                ServiceLifetime.Singleton);
            services.Replace(descriptor); // Replace existing activator

            services.AddLogging();

            // Add Custom Services
            services.AddScoped<IMapper<DataAvailable, DataAvailableCommand>, DataAvailableMapper>();
            services.AddScoped<IMessageReplyStorage, MessageReplyTableStorage>();
            services.AddTransient<IGetContentPathStrategy, ContentPathFromSavedResponse>();
            services.AddTransient<IGetContentPathStrategy, ContentPathFromSubDomain>();
            services.AddScoped<IGetContentPathStrategyFactory, GetContentPathStrategyFactory>();
            services.AddScoped<ISendMessageToServiceBus, SendMessageToServiceBus>();
            services.AddScoped<IGetPathToDataFromServiceBus, GetPathToDataFromServiceBus>();

            services.AddSingleton(_ => new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString")));
            services.AddDatabaseCosmosConfig();
            services.AddServiceBusConfig();
            services.AddCosmosClientBuilder(useBulkExecution: false);

            Configure(services);

            services.AddSimpleInjector(Container);

            // SimpleInjector registrations
            Container.Register<IValidator<DataAvailableCommand>, DataAvailableRuleSet>(Lifestyle.Scoped);
            Container.Register<IDataAvailableController, DataAvailableController>(Lifestyle.Scoped);
            Container.Register<IStorageService, StorageService>(Lifestyle.Scoped);
            Container.Register<IDataAvailableRepository, DataAvailableRepository>(Lifestyle.Scoped);

            // Add MediatR
            // todo mmj
            Container.BuildMediator(new[] { typeof(DataAvailableHandler).Assembly }, Array.Empty<Type>());

            Configure(Container);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

#pragma warning disable VSTHRD200
        protected virtual ValueTask DisposeAsyncCore()
#pragma warning restore VSTHRD200
        {
            return Container.DisposeAsync();
        }

        protected abstract void Configure(Container container);
        protected abstract void Configure(IServiceCollection serviceCollection);
    }
}
