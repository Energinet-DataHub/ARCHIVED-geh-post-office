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
using System.Buffers;
using System.Text.Json;
using TestJSonConversion.SimpleCimJson.Factories;

namespace TestJSonConversion.SimpleCimJson.Elements;

public sealed class CimObjectElement : ICimElement
{
    private ICimElement?[] _currentElements;

    public void SetSize(int capacity)
    {
        _currentElements = ArrayPool<ICimElement?>.Shared.Rent(capacity);
        Array.Clear(_currentElements, 0, _currentElements.Length);
    }

    public void AddElement(int index, ICimElement element)
    {
        _currentElements[index] = element;
    }

    public string Key { get; set; }

    public void WriteJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WriteStartObject(Key);
        for (var i = 0; i < _currentElements.Length; i++)
        {
            if (_currentElements[i] is not null)
            {
                _currentElements[i]?.WriteJson(jsonWriter);
                _currentElements[i]?.ReturnToPool();
            }
        }
        jsonWriter.WriteEndObject();
        Array.Clear(_currentElements, 0, _currentElements.Length);
    }

    public void ReturnToPool()
    {
        CimJsonObjectPools.ReturnElement(this);
        ArrayPool<ICimElement?>.Shared.Return(_currentElements);
    }
}