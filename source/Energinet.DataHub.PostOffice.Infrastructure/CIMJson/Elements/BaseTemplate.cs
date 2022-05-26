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
using TestJSonConversion.SimpleCimJson.Factories;
using TestJSonConversion.SimpleCimJson.Reader;

namespace TestJSonConversion.SimpleCimJson.Elements;

internal abstract class BaseTemplate
{
    private readonly ArrayBufferWriter<byte> _jsonStream = new();
    protected Utf8JsonWriter _jsonWriter;
    protected CimXmlReader _reader;

    public ReadOnlyMemory<byte> ConvertXmlToJson(Stream xmlData)
    {
        using var reader = new CimXmlReader(xmlData);
        using Utf8JsonWriter jsonWriter = new(_jsonStream, new JsonWriterOptions { Indented = false, SkipValidation = true});
        _jsonWriter = jsonWriter;
        _reader = reader;
        Convert();
        jsonWriter.Flush();
        return _jsonStream.WrittenMemory;
    }
    protected abstract void Convert();

    public string GetTotalUsedBuffer() => _reader.GetTotalMemoryUsedInBuffers();
    public string GetTotalBuffer() => _reader.GetTotalBufferSize();
    public int GetTotalNumberOfBuffers() => _reader.GetTotalNumberOfBuffers();
    public string GetIndividualBufferSizes() => _reader.GetIndividualBufferSizes();

    protected void DirectWriteSimpleStringSegment(string elementName)
    {
        _jsonWriter.WriteString(elementName, _reader.ReadValue().Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CimStringValueElement WriteAsString(string elementName)
    {
       return  CimJsonObjectPools.GetStringValueElement(elementName, _reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CimIntValueElement WriteAsNumber(string elementName)
    {
        return  CimJsonObjectPools.GetIntValueElement(elementName, _reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CimBoolValueElement WriteAsBool(string elementName)
    {
        return  CimJsonObjectPools.GetBoolValueElement(elementName, _reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CimObjectElement WriteAsStringValueObject(string elementName)
    {
        var element = CimJsonObjectPools.GetObjectElement(elementName, 1);
        element.AddElement(0, CimJsonObjectPools.GetStringValueElement(GenericElementNames.Value, _reader));
        return element;
    }

    protected CimObjectElement WriteObjectWithCodingSchemeElement(string elementName)
    {
        var element = CimJsonObjectPools.GetObjectElement(elementName, 2);
        do
        {
            switch (_reader.CurrentAttributeName)
            {
                case GenericElementNames.Attributes.CodingScheme:
                    element.AddElement(0, CimJsonObjectPools.GetStringValueElement(GenericElementNames.Attributes.CodingScheme, _reader));
                    break;
            }
        } while (_reader.AdvanceAttribute());
        do
        {
            if (_reader.CurrentNodeType != NodeType.StartElement ) continue;
            element.AddElement(1, CimJsonObjectPools.GetStringValueElement(GenericElementNames.Value, _reader));
        } while (_reader.AdvanceUntilClosed(elementName));
        return element;
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