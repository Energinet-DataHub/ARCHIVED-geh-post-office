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
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Reader;

public sealed class CimXmlReader : ICimXmlReader, IDisposable
{
    private readonly Stream _inputStream;
    private XmlReader? _xmlReader;
    private string? _attributeValue;
    private string? _emptyElementTag;

    public CimXmlReader(Stream inputStream)
    {
        _inputStream = inputStream;
        CurrentNodeName = string.Empty;
        CurrentNodeType = NodeType.None;
        CanReadValue = false;
    }

    public string CurrentNodeName { get; private set; }
    public bool CanReadValue { get; private set; }
    public NodeType CurrentNodeType { get; private set; }

    public async Task<bool> AdvanceAsync()
    {
        EnsureReader();

        if (_emptyElementTag != null)
        {
            ProcessEmptyElement(_emptyElementTag);
            _emptyElementTag = null;
            return true;
        }

        do
        {
            _attributeValue = null;

            switch (_xmlReader!.NodeType)
            {
                case XmlNodeType.Element:
                    ProcessElement();
                    return true;
                case XmlNodeType.EndElement:
                    ProcessEndElement();
                    return true;
                case XmlNodeType.Attribute:
                    ProcessAttribute();
                    return true;
            }
        }
        while (await ValidatingReadAsync().ConfigureAwait(false));

        CurrentNodeName = string.Empty;
        CurrentNodeType = NodeType.None;
        CanReadValue = false;

        return false;
    }

    public Task<string> ReadValueAsStringAsync()
    {
        return ReadValueAsAsync(x => x);
    }

    public void Dispose()
    {
        _inputStream.Dispose();
        _xmlReader?.Dispose();
    }

    private async Task<T> ReadValueAsAsync<T>(Func<string, T> attributeFunc)
    {
        EnsureReader();

        if (_attributeValue != null)
        {
            return attributeFunc(_attributeValue);
        }

        var content = await _xmlReader!
            .ReadContentAsAsync(typeof(T), null)
            .ConfigureAwait(false);

        return (T)content;
    }

    private async Task<bool> ValidatingReadAsync()
    {
        EnsureReader();
        bool couldRead;

        do
        {
            couldRead = await _xmlReader!.ReadAsync().ConfigureAwait(false);

            // If could read without errors, return true.
            // Otherwise, read to end to get all the errors.
            if (couldRead)
            {
                return true;
            }
        }
        while (couldRead);

        return false;
    }

    private void ProcessAttribute()
    {
        _attributeValue = _xmlReader!.Value;
        CurrentNodeName = _xmlReader.LocalName;
        CurrentNodeType = NodeType.Attribute;
        CanReadValue = true;

        // Advance to next attribute.
        if (_xmlReader.MoveToNextAttribute())
        {
            return;
        }

        // Once we are out of attributes, read to next element.
        _xmlReader.MoveToElement();
        HandleEmptyElement();
    }

    private void ProcessElement()
    {
        CurrentNodeName = _xmlReader!.LocalName;
        CurrentNodeType = NodeType.StartElement;
        CanReadValue = false;

        if (_xmlReader.HasAttributes)
        {
            _xmlReader.MoveToNextAttribute();
            return;
        }

        if (HandleEmptyElement())
        {
            return;
        }

        if (_xmlReader!.NodeType == XmlNodeType.Text)
        {
            CanReadValue = true;
        }
    }

    private void ProcessEmptyElement(string closedTag)
    {
        CurrentNodeName = closedTag;
        CurrentNodeType = NodeType.EndElement;
        CanReadValue = false;
        _xmlReader!.ReadStartElement();
    }

    private void ProcessEndElement()
    {
        CurrentNodeName = _xmlReader!.LocalName;
        CurrentNodeType = NodeType.EndElement;
        CanReadValue = false;
        _xmlReader.ReadEndElement();
    }

    private bool HandleEmptyElement()
    {
        if (_xmlReader!.IsEmptyElement)
        {
            _emptyElementTag = _xmlReader.LocalName;
            return true;
        }

        _xmlReader.ReadStartElement();
        return false;
    }

    private void EnsureReader()
    {
        if (_xmlReader != null) return;
        var xmlReaderSettings = new XmlReaderSettings
        {
            Async = true,
            CheckCharacters = false,
            CloseInput = true,
            ConformanceLevel = ConformanceLevel.Auto,
            IgnoreWhitespace = true
        };

        _xmlReader = XmlReader.Create(_inputStream, xmlReaderSettings);
    }
}
