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
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements.ValueElements;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Reader;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements.Containers;

internal sealed class CimObjectElement : ICimElement, IDisposable
{
    private (bool HasValue, ICimElement? Element)[] _currentElements;

    public CimObjectElement(string key, int capacity)
    {
        _currentElements = ArrayPool<(bool HasValue, ICimElement? Element)>.Shared.Rent(capacity);
        Array.Clear(_currentElements, 0, _currentElements.Length);
        Key = key;
    }

    public string Key { get; }

    public CimArrayElement AddArray(string key, int capacity, int location)
    {
        if (_currentElements[location].Element is CimArrayElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            // it's the same array, clear it and return, so we reuse it
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

    public CimStringValueElement AddString(string key, int location, CimXmlReader reader)
    {
        if (_currentElements[location].Element is CimStringValueElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            current.SetValue(reader.ReadValue());
            _currentElements[location].HasValue = true;
            return current;
        }

        // Add new string value element
        var stringValue = new CimStringValueElement(key, reader.ReadValue());
        _currentElements[location].Element = stringValue;
        _currentElements[location].HasValue = true;
        return stringValue;
    }

    public CimObjectValueElement AddObjectWithValueString(string key, int location, CimXmlReader reader)
    {
        if (_currentElements[location].Element is CimObjectValueElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            current.SetString(reader);
            _currentElements[location].HasValue = true;
            return current;
        }

        // Add new object with value element
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

        // Add new Integer value element
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

        // Add new object with codingscheme and value element
        var objectWithValue = new CimObjectWithCodingSchemeElement(key);
        objectWithValue.Update(reader);
        _currentElements[location].Element = objectWithValue;
        _currentElements[location].HasValue = true;

        return objectWithValue;
    }

    public CimIntValueElement AddInteger(string key, int location, CimXmlReader reader)
    {
        if (_currentElements[location].Element is CimIntValueElement current && string.Equals(key, current.Key, StringComparison.Ordinal))
        {
            current.SetValue(reader.ReadValue());
            _currentElements[location].HasValue = true;
            return current;
        }

        // Add new integer value
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

        // Add new boolean value
        var boolValue = new CimBoolValueElement(key, reader.ReadValue());
        _currentElements[location].Element = boolValue;
        _currentElements[location].HasValue = true;

        return boolValue;
    }

    public void WriteJson(Utf8JsonWriter jsonWriter)
    {
        ArgumentNullException.ThrowIfNull(jsonWriter, nameof(jsonWriter));
        jsonWriter.WriteStartObject(Key);
        for (var i = 0; i < _currentElements.Length; i++)
        {
            var current = _currentElements[i];
            if (current.Element is not null && current.HasValue)
            {
                current.Element.WriteJson(jsonWriter);
                _currentElements[i].HasValue = false;
            }
        }

        jsonWriter.WriteEndObject();
    }

    public void Dispose()
    {
        for (var i = 0; i < _currentElements.Length; i++)
        {
            if (_currentElements[i].Element is not null && _currentElements[i].Element is IDisposable disposable)
                disposable.Dispose();
        }

        ArrayPool<(bool HasValue, ICimElement? Element)>.Shared.Return(_currentElements, true);
    }
}
