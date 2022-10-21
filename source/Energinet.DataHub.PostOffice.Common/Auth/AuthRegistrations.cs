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
using System.IdentityModel.Tokens.Jwt;
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.PostOffice.Common.Configuration;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common.Auth
{
    public static class HttpAuthenticationRegistrations
    {
        private static readonly string[] _functionNamesToExclude = { "HealthCheck" };

        public static void AddHttpAuthentication(this Container container)
        {
            ArgumentNullException.ThrowIfNull(container, nameof(container));

            container.Register<IMarketOperatorIdentity, MarketOperatorIdentity>(Lifestyle.Scoped);
            container.Register<JwtAuthenticationMiddleware>(Lifestyle.Scoped);
            RegisterJwt(container);
            RegisterActor(container);
        }

        private static void RegisterJwt(Container container)
        {
            container.Register<IConfigurationManager<OpenIdConnectConfiguration>>(
                () =>
                {
                    var configuration = container.GetService<IConfiguration>();
                    var tenantId = configuration.GetSetting(Settings.OpenIdTenantId);
                    return new ConfigurationManager<OpenIdConnectConfiguration>(
                        $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration",
                        new OpenIdConnectConfigurationRetriever());
                },
                Lifestyle.Singleton);

            container.Register<ISecurityTokenValidator, JwtSecurityTokenHandler>(Lifestyle.Singleton);
            container.Register<IJwtTokenValidator>(
                () =>
                {
                    var configuration = container.GetService<IConfiguration>();
                    var audience = configuration.GetSetting(Settings.OpenIdAudience);
                    return new JwtTokenValidator(
                        container.GetRequiredService<ILogger<JwtTokenValidator>>(),
                        container.GetRequiredService<ISecurityTokenValidator>(),
                        container.GetRequiredService<IConfigurationManager<OpenIdConnectConfiguration>>(),
                        audience);
                },
                Lifestyle.Scoped);

            container.Register<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>(Lifestyle.Scoped);
            container.Register<ClaimsPrincipalContext>(Lifestyle.Scoped);
            container.Register(
                () =>
                {
                    var claimsPrincipalContext = container.GetRequiredService<ClaimsPrincipalContext>();
                    var jwtTokenValidator = container.GetRequiredService<IJwtTokenValidator>();
                    return new JwtTokenMiddleware(claimsPrincipalContext, jwtTokenValidator);
                },
                Lifestyle.Scoped);
        }

        private static void RegisterActor(Container container)
        {
            container.Register<LegacyActorProvider>(Lifestyle.Scoped);
            container.Register<ActorRegistryProvider>(Lifestyle.Scoped);
            container.Register<IActorContext, ActorContext>(Lifestyle.Scoped);
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
