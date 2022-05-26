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

public sealed class CimArrayElement : ICimElement
{
    private readonly ArrayBufferWriter<byte> _arrayBuffer;
    private readonly ICimElement?[] _currentElements;
    private Utf8JsonWriter? _jsonWriter;

    public string Key { get; set; }
    public bool HasElements { get; private set; }

    public CimArrayElement(string key, int maxPossibleElements)
    {
        Key = key;
        _currentElements = ArrayPool<ICimElement?>.Shared.Rent(maxPossibleElements);
        Array.Clear(_currentElements, 0, _currentElements.Length);
        _arrayBuffer = CimJsonObjectPools.GetArrayWriter();
        _arrayBuffer.Clear();
    }
    public void BeginNewArrayElement()
    {
        EnsureWriter();
        if (!HasElements)
        {
            _jsonWriter!.WriteStartArray(Key);
            HasElements = true;
        }
        else
        {
            WriteElements();
        }
    }

    private void EnsureWriter()
    {
        _jsonWriter ??= new Utf8JsonWriter(_arrayBuffer,
            new JsonWriterOptions() { Indented = false, SkipValidation = true });
    }

    private void WriteElements()
    {
        _jsonWriter!.WriteStartObject();
        foreach (var  element in _currentElements)
        {
            if (element is not null)
            {
                element.WriteJson(_jsonWriter!);
                element.ReturnToPool();
            }

        }
        _jsonWriter!.WriteEndObject();
        Array.Clear(_currentElements, 0, _currentElements.Length);
    }


    public void WriteJson(Utf8JsonWriter jsonWriter)
    {
        EnsureWriter();
        WriteElements();
        _jsonWriter!.WriteEndArray();
        _jsonWriter.Flush();
        jsonWriter.WriteRawValue(_arrayBuffer.WrittenSpan, true);
    }

    public void ReturnToPool()
    {
        ArrayPool<ICimElement?>.Shared.Return(_currentElements);
        _jsonWriter?.Dispose();
        CimJsonObjectPools.ReturnElement(_arrayBuffer);
    }

    public void AddElement(int location, ICimElement element)
    {
        _currentElements[location] =  element;
    }
}