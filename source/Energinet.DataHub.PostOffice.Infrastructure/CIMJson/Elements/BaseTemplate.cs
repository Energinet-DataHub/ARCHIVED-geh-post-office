// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Factories;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Reader;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements;

internal abstract class BaseTemplate : IDisposable
{
    private readonly ArrayBufferWriter<byte> _jsonStream = new();
    private CimXmlReader? _reader;
    private Utf8JsonWriter? _jsonWriter;
    private bool _isDisposed;

    public ReadOnlyMemory<byte> ConvertXmlToJson(Stream xmlData)
    {
        _reader = new CimXmlReader(xmlData);
        _jsonWriter = new Utf8JsonWriter(_jsonStream, new JsonWriterOptions { Indented = false, SkipValidation = true });
        Convert();
        _jsonWriter.Flush();
        return _jsonStream.WrittenMemory;
    }

    public void Dispose()
    {
        Dispose(true);
    }

    protected abstract void Convert();

    protected Utf8JsonWriter JsonWriter() => _jsonWriter!;
    protected CimXmlReader CimReader() => _reader!;

    protected bool Advance()
    {
        return _reader!.Advance();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CimStringValueElement WriteAsString(string elementName)
    {
        if (_reader is null)
            throw new InvalidOperationException("Error in conversion, _reader was null");

        return CimJsonObjectPools.GetStringValueElement(elementName, _reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CimIntValueElement WriteAsNumber(string elementName)
    {
        if (_reader is null)
            throw new InvalidOperationException("Error in conversion, _reader was null");

        return CimJsonObjectPools.GetIntValueElement(elementName, _reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CimBoolValueElement WriteAsBool(string elementName)
    {
        if (_reader is null)
            throw new InvalidOperationException("Error in conversion, _reader was null");

        return CimJsonObjectPools.GetBoolValueElement(elementName, _reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CimObjectElement WriteAsStringValueObject(string elementName)
    {
        if (_reader is null)
            throw new InvalidOperationException("Error in conversion, _reader was null");

        var element = CimJsonObjectPools.GetObjectElement(elementName, 1);
        element.AddElement(0, CimJsonObjectPools.GetStringValueElement(GenericElementNames.Value, _reader));
        return element;
    }

    protected CimObjectElement WriteObjectWithCodingSchemeElement(string elementName)
    {
        if (_reader is null)
            throw new InvalidOperationException("Error in conversion, _reader was null");

        var element = CimJsonObjectPools.GetObjectElement(elementName, 2);
        do
        {
            switch (_reader.CurrentAttributeName)
            {
                case GenericElementNames.Attributes.CodingScheme:
                    element.AddElement(0, CimJsonObjectPools.GetStringValueElement(GenericElementNames.Attributes.CodingScheme, _reader));
                    break;
            }
        }
        while (_reader.AdvanceAttribute());

        do
        {
            if (_reader.CurrentNodeType != NodeType.StartElement) continue;
            element.AddElement(1, CimJsonObjectPools.GetStringValueElement(GenericElementNames.Value, _reader));
        }
        while (_reader.AdvanceUntilClosed(elementName));

        return element;
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            // free managed resources
            _reader?.Dispose();
            _jsonWriter?.Dispose();
        }

        _isDisposed = true;
    }

    protected static class GenericElementNames
    {
        public const string Value = "value";

        public static class Attributes
        {
            public const string CodingScheme = "codingScheme";
            public const string Unit = "unit";
        }
    }
}
