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
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.PostOffice.Infrastructure.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common.Auth
{
    public static class HttpAuthenticationRegistrations
    {
        private static readonly string[] _functionNamesToExclude = { "HealthCheck", };

        public static void AddHttpAuthentication(this Container container)
        {
            ArgumentNullException.ThrowIfNull(container, nameof(container));

            container.Register<IMarketOperatorIdentity, MarketOperatorIdentity>(Lifestyle.Scoped);
            container.Register<JwtAuthenticationMiddleware>(Lifestyle.Scoped);
            RegisterJwt(container);
            RegisterActor(container);

            container.AddMarketParticipantConfig();
        }

        public static void AddMarketParticipantConfig(this Container container)
        {
            ArgumentNullException.ThrowIfNull(container, nameof(container));

            container.Register(() =>
            {
                const string connectionStringKey = "SQL_ACTOR_DB_CONNECTION_STRING";
                var connectionString = Environment.GetEnvironmentVariable(connectionStringKey) ?? throw new InvalidOperationException($"{connectionStringKey} is required");
                return new ActorDbConfig(connectionString);
            });
        }

        private static void RegisterJwt(Container container)
        {
            container.Register<IJwtTokenValidator, JwtTokenValidator>(Lifestyle.Scoped);
            container.Register<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>(Lifestyle.Scoped);
            container.Register<ClaimsPrincipalContext>(Lifestyle.Scoped);
            container.Register(
                () => new JwtTokenMiddleware(
                    container.GetRequiredService<ClaimsPrincipalContext>(),
                    container.GetRequiredService<IJwtTokenValidator>(),
                    _functionNamesToExclude),
                Lifestyle.Scoped);

            container.Register(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                var tenantId = configuration.GetValue<string>("B2C_TENANT_ID") ?? throw new InvalidOperationException("B2C tenant id not found.");
                var audience = configuration.GetValue<string>("BACKEND_SERVICE_APP_ID") ?? throw new InvalidOperationException("Backend service app id not found.");
                return new OpenIdSettings($"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration", audience);
            });
        }

        private static void RegisterActor(Container container)
        {
            container.Register<IActorContext, ActorContext>(Lifestyle.Scoped);
            container.Register<LegacyActorProvider>(Lifestyle.Scoped);
            container.Register<ActorRegistryProvider>(Lifestyle.Scoped);
            container.Register<LegacyActorIdIdentity>(Lifestyle.Scoped);
            container.Register<IActorProvider, LegacyActorProviderProxy>(Lifestyle.Scoped);

            container.Register(
                () => new ActorMiddleware(
                    container.GetRequiredService<IClaimsPrincipalAccessor>(),
                    container.GetRequiredService<IActorProvider>(),
                    container.GetRequiredService<IActorContext>(),
                    _functionNamesToExclude),
                Lifestyle.Scoped);
        }
    }
}
