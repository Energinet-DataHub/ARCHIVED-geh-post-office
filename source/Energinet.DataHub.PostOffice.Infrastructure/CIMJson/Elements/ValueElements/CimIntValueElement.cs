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
using System.Text.Json;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements.Interfaces;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements.ValueElements;

internal class CimIntValueElement : ICimValueElement
{
    private int _value;
    public CimIntValueElement(string key, ReadOnlyMemory<char> value)
    {
        Key = key;
        _value = int.Parse(value.Span);
    }

    public string Key { get; }

    public void WriteJson(Utf8JsonWriter jsonWriter)
    {
        ArgumentNullException.ThrowIfNull(jsonWriter, nameof(jsonWriter));
        jsonWriter.WriteNumber(Key.AsSpan(), _value);
    }

    public void SetValue(ReadOnlyMemory<char> value)
    {
        _value = int.Parse(value.Span);
    }
}
