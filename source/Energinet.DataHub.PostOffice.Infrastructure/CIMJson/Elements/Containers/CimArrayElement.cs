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
using System.Text.Encodings.Web;
using System.Text.Json;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements.ValueElements;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Reader;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements.Containers;

internal sealed class CimArrayElement : ICimElement, IDisposable
{
    private readonly (bool HasValue, ICimElement? Element)[] _currentElements;
    private Utf8JsonWriter _jsonWriter;
    private bool _hasElements;
    private ArrayBufferWriter<byte> _arrayStream;

    public CimArrayElement(string key, int capacity)
    {
        Key = key;
        _currentElements = ArrayPool<(bool IsUpdated, ICimElement? Element)>.Shared.Rent(capacity);
        _arrayStream = new ArrayBufferWriter<byte>();
        _jsonWriter = new Utf8JsonWriter(
            _arrayStream,
            new JsonWriterOptions { Indented = false, SkipValidation = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        _hasElements = false;
    }

    public string Key { get; }

    public void BeginNewArrayElement()
    {
        if (!_hasElements)
        {
            _jsonWriter.WriteStartArray(Key);
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
        WriteElements();
        _jsonWriter.WriteEndArray();
        _jsonWriter.Flush();
        jsonWriter.WriteRawValue(_arrayStream.WrittenSpan, true);
    }

    public CimArrayElement AddArray(string key, int capacity, int location)
    {
        if (_currentElements[location].Element is CimArrayElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            current.ReInit();
            return current;
        }

        // Add new Array
        var array = new CimArrayElement(key, capacity);
        _currentElements[location].Element = array;
        _currentElements[location].HasValue = true;
        return array;
    }

    public CimObjectElement AddObject(string key, int capacity, int location)
    {
        if (_currentElements[location].Element is CimObjectElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            // it's the same array, clear it and return, so we reuse it
            return current;
        }

        // Add new object element
        var objectElement = new CimObjectElement(key, capacity);
        _currentElements[location].Element = objectElement;
        _currentElements[location].HasValue = true;

        return objectElement;
    }

    public CimObjectValueElement AddObjectWithValueString(string key, int location, CimXmlReader reader)
    {
        if (_currentElements[location].Element is CimObjectValueElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            current.SetString(reader);
            _currentElements[location].HasValue = true;
            return current;
        }

        //Add new string object value element
        var objectWithValue = new CimObjectValueElement(key);
        objectWithValue.SetString(reader);
        _currentElements[location].Element = objectWithValue;
        _currentElements[location].HasValue = true;

        return objectWithValue;
    }

    public CimObjectValueElement AddObjectWithValueInteger(string key, int location, CimXmlReader reader)
    {
        if (_currentElements[location].Element is CimObjectValueElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            current.SetInteger(reader);
            _currentElements[location].HasValue = true;
            return current;
        }

        // Add new integer object value element
        var objectWithValue = new CimObjectValueElement(key);
        objectWithValue.SetInteger(reader);
        _currentElements[location].Element = objectWithValue;
        _currentElements[location].HasValue = true;

        return objectWithValue;
    }

    public CimObjectWithCodingSchemeElement AddObjectWithCodingScheme(string key, int location, CimXmlReader reader)
    {
        if (_currentElements[location].Element is CimObjectWithCodingSchemeElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            current.Update(reader);
            _currentElements[location].HasValue = true;
            return current;
        }

        // Add new string value
        var objectWithValue = new CimObjectWithCodingSchemeElement(key);
        objectWithValue.Update(reader);
        _currentElements[location].Element = objectWithValue;
        _currentElements[location].HasValue = true;

        return objectWithValue;
    }

    public CimStringValueElement AddString(string key, int location, CimXmlReader reader)
    {
        if (_currentElements[location].Element is CimStringValueElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            current.SetValue(reader.ReadValue());
            _currentElements[location].HasValue = true;
            return current;
        }

        // Add new string value
        var stringValue = new CimStringValueElement(key, reader.ReadValue());
        _currentElements[location].Element = stringValue;
        _currentElements[location].HasValue = true;
        return stringValue;
    }

    public CimIntValueElement AddInteger(string key, int location, CimXmlReader reader)
    {
        if (_currentElements[location].Element is CimIntValueElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            current.SetValue(reader.ReadValue());
            _currentElements[location].HasValue = true;
            return current;
        }

        // Add new string value
        var intValue = new CimIntValueElement(key, reader.ReadValue());
        _currentElements[location].Element = intValue;
        _currentElements[location].HasValue = true;

        return intValue;
    }

    public CimBoolValueElement AddBool(string key, int location, CimXmlReader reader)
    {
        if (_currentElements[location].Element is CimBoolValueElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            current.SetValue(reader.ReadValue());
            _currentElements[location].HasValue = true;
            return current;
        }

        //Add new string value
        var boolValue = new CimBoolValueElement(key, reader.ReadValue());
        _currentElements[location].Element = boolValue;
        _currentElements[location].HasValue = true;

        return boolValue;
    }

    public void ReInit()
    {
        _arrayStream.Clear();
        _jsonWriter.Reset();
        _hasElements = false;
    }

    public void Dispose()
    {
        for (var i = 0; i < _currentElements.Length; i++)
        {
            if (_currentElements[i].Element is not null && _currentElements[i].Element is IDisposable disposable)
                disposable.Dispose();
        }

        ArrayPool<(bool IsUpdated, ICimElement? Element)>.Shared.Return(_currentElements, true);
        _jsonWriter.Dispose();
        _arrayStream.Clear();
    }

    private void WriteElements()
    {
        _jsonWriter.WriteStartObject();
        for (var i = 0; i < _currentElements.Length; i++)
        {
            var current = _currentElements[i];
            if (current.Element is not null && current.HasValue)
            {
                current.Element.WriteJson(_jsonWriter);
                _currentElements[i].HasValue = false;
            }
        }

        _jsonWriter.WriteEndObject();
    }
}
