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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.PostOffice.Common.Configuration;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.PostOffice.Common.Auth
{
    public sealed class LegacyActorProvider : IActorProvider
    {
        private readonly IConfiguration _configuration;

        public LegacyActorProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Issue: https://github.com/dotnet/roslyn-analyzers/issues/5712")]
        public async Task<Actor> GetActorAsync(Guid actorId)
        {
            const string param = "ACTOR_ID";
            const string query = @"SELECT TOP 1 [Id]
                            ,[IdentificationType]
                            ,[IdentificationNumber]
                            ,[Roles]
                        FROM  [dbo].[ActorInfo]
                        WHERE Id = @" + param;

            var legacyConnectionString = _configuration.GetSetting(Settings.SqlActorDbConnectionString);

            await using var connection = new SqlConnection(legacyConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection)
            {
                Parameters = { new SqlParameter(param, actorId) }
            };

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var record = (IDataRecord)reader;

                return new Actor(
                    record.GetGuid(0),
                    record.GetInt32(1).ToString(CultureInfo.InvariantCulture),
                    record.GetString(2),
                    record.GetString(3));
            }

            throw new InvalidOperationException($"Actor with id {actorId} not found.");
        }
    }
}
