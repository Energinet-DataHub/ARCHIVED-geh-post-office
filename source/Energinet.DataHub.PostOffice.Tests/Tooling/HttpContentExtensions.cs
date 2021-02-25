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
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Energinet.DataHub.PostOffice.Tests.Tooling
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content, JsonSerializerSettings jsonSerializerSettings = null!)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (jsonSerializerSettings == null)
            {
                jsonSerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DateParseHandling = DateParseHandling.None,
                }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            }

            string json = await content.ReadAsStringAsync().ConfigureAwait(false);
            T value = JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
            return value;
        }
    }
}
