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
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Factories;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements;

public sealed class CimArrayElement : ICimElement, IDisposable
{
    private ArrayBufferWriter<byte>? _arrayBuffer;
    private ICimElement?[]? _currentElements;
    private Utf8JsonWriter? _jsonWriter;
    private bool _hasElements;
    private string _key = string.Empty;

    public void Initialize(string key, int maxPossibleElements)
    {
        _key = key;
        _currentElements = ArrayPool<ICimElement?>.Shared.Rent(maxPossibleElements);
        Array.Clear(_currentElements, 0, _currentElements.Length);
        _arrayBuffer = CimJsonObjectPools.GetArrayWriter();
        _arrayBuffer.Clear();
        EnsureWriter(true);
    }

    public void BeginNewArrayElement()
    {
        EnsureWriter();
        if (!_hasElements)
        {
            _jsonWriter!.WriteStartArray(_key);
            _hasElements = true;
        }
        else
        {
            WriteElements();
        }
    }

    public void WriteJson(Utf8JsonWriter jsonWriter)
    {
        ArgumentNullException.ThrowIfNull(jsonWriter);
        if (_arrayBuffer is null)
            throw new InvalidOperationException($"_arraybuffer was null, ensure that {nameof(CimArrayElement)}->{nameof(Initialize)} has been called ");

        EnsureWriter();
        WriteElements();
        _jsonWriter!.WriteEndArray();
        _jsonWriter.Flush();
        jsonWriter.WriteRawValue(_arrayBuffer.WrittenSpan, true);
    }

    public void AddElement(int location, ICimElement element)
    {
        if (_currentElements is null || _currentElements.Length >= location)
            throw new InvalidOperationException($"_currentElements was null or incorrect length, ensure that {nameof(CimArrayElement)}->{nameof(Initialize)} has been called ");

        _currentElements[location] = element;
    }

    public void ReturnToPool()
    {
        if (_currentElements is not null)
            ArrayPool<ICimElement?>.Shared.Return(_currentElements);

        if (_arrayBuffer is not null)
            CimJsonObjectPools.ReturnElement(_arrayBuffer);

        CimJsonObjectPools.ReturnElement(this);
    }

    public void Dispose()
    {
        _jsonWriter?.Dispose();
    }

    private void EnsureWriter(bool forceNew = false)
    {
        if (_arrayBuffer is null)
            throw new InvalidOperationException($"_arraybuffer was null, ensure that {nameof(CimArrayElement)}->{nameof(Initialize)} has been called ");

        if (forceNew)
        {
            _jsonWriter?.Dispose();
            _jsonWriter = null;
        }

        _jsonWriter ??= new Utf8JsonWriter(
            _arrayBuffer,
            new JsonWriterOptions { Indented = false, SkipValidation = true });
    }

    private void WriteElements()
    {
        if (_currentElements is null)
            throw new InvalidOperationException($"_currentElements was null or incorrect length, ensure that {nameof(CimArrayElement)}->{nameof(Initialize)} has been called ");

        if (_jsonWriter is null)
            throw new InvalidOperationException($"_currentElements was null or incorrect length, ensure that {nameof(CimArrayElement)}->{nameof(Initialize)} has been called ");

        _jsonWriter!.WriteStartObject();
        foreach (var element in _currentElements)
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
}
