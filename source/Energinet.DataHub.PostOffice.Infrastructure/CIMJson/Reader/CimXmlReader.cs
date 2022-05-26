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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Reader;

public sealed class CimXmlReader : ICimXmlReader, IDisposable
{
    private const int BufferSize = 128;
    private readonly Stream _inputStream;
    private readonly List<char[]> _buffers = new() { ArrayPool<char>.Shared.Rent(BufferSize) };
    private XmlReader? _xmlReader;
    private string? _emptyElementTag;
    private int _index;
    private int _buffersIndex;
    private bool _hasReachedNewNode;

    public CimXmlReader(Stream inputStream)
    {
        _inputStream = inputStream;
        CurrentAttributeName = string.Empty;
        CurrentNodeName = string.Empty;
        CurrentNodeType = NodeType.None;
        CanReadValue = false;
    }

    public string CurrentNodeName { get; private set; }
    public string CurrentAttributeName { get; private set; }
    public bool CanReadValue { get; private set; }
    public NodeType CurrentNodeType { get; private set; }
    public bool HasAttributes { get; private set; }

    public bool Advance()
    {
        EnsureReader();

        if (_hasReachedNewNode)
        {
            _hasReachedNewNode = false;
            CurrentNodeName = _xmlReader!.LocalName;
            CurrentNodeType = NodeType.StartElement;
            CanReadValue = false;
            var depthBefore = _xmlReader.Depth;
            if (_xmlReader.IsStartElement())
            {
                _xmlReader.ReadStartElement();
                if (_xmlReader.NodeType == XmlNodeType.Element && depthBefore < _xmlReader.Depth)
                {
                    _hasReachedNewNode = true;
                    return true;
                }

                CanReadValue = true;
            }

            return true;
        }

        if (_emptyElementTag != null)
        {
            ProcessEmptyElement(_emptyElementTag);
            _emptyElementTag = null;
            return true;
        }

        CurrentAttributeName = string.Empty;
        while (ValidatingRead())
        {
            switch (_xmlReader!.NodeType)
            {
                case XmlNodeType.Element:
                    ProcessElement();
                    return true;
                case XmlNodeType.EndElement:
                    ProcessEndElement();
                    return true;
                case XmlNodeType.None:
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.EntityReference:
                case XmlNodeType.Entity:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.Document:
                case XmlNodeType.DocumentType:
                case XmlNodeType.DocumentFragment:
                case XmlNodeType.Notation:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.EndEntity:
                case XmlNodeType.XmlDeclaration:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        CurrentNodeName = string.Empty;
        CurrentNodeType = NodeType.None;
        CanReadValue = false;
        return false;
    }

    public bool AdvanceAttribute()
    {
        EnsureReader();
        if (!_xmlReader!.HasAttributes)
            return false;

        if (!_xmlReader.MoveToNextAttribute())
        {
            _xmlReader.MoveToContent();
            _xmlReader.ReadStartElement();
            return false;
        }

        CurrentAttributeName = _xmlReader.LocalName;
        return true;
    }

    public bool AdvanceUntilClosed(string nodeName)
    {
        while (Advance())
        {
            if (CurrentNodeType == NodeType.EndElement)
            {
                if (CurrentNodeName.Equals(nodeName, StringComparison.Ordinal)) break;

                if (CanReadValue) return true;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public ReadOnlyMemory<char> ReadValue()
    {
        EnsureReader();
        int read;
        var total = 0;
        var startIndex = _index;
        while ((read = _xmlReader!.ReadValueChunk(_buffers[_buffersIndex], _index, _buffers[_buffersIndex].Length - _index)) > 0)
        {
            total += read;
            _index += read;
            //Check if we filled the entire buffer with this read, and expand if necessary
            if (_buffers[_buffersIndex].Length - _index <= 0)
            {
                ExpandBuffers(startIndex);
                startIndex = 0;
            }
        }

        //Check if we exactly filled the entire buffer with this read, and expand if necessary
        if (_buffers[_buffersIndex].Length - _index <= 0)
        {
            ExpandBuffers(startIndex, false);
            _index = 0;
        }

        return _buffers[_buffersIndex].AsMemory(startIndex, total); //.Slice(startIndex, total);
    }

    public void Dispose()
    {
        _inputStream.Dispose();
        _xmlReader?.Dispose();
        if (!_buffers.Any()) return;
        foreach (var buffer in _buffers) ArrayPool<char>.Shared.Return(buffer);
    }

    private bool ValidatingRead()
    {
        EnsureReader();
        bool couldRead;

        do
        {
            couldRead = _xmlReader!.Read();

            // If could read without errors, return true.
            // Otherwise, read to end to get all the errors.
            if (couldRead) return true;
        }
        while (couldRead);

        return false;
    }

    private void ExpandBuffers(int startIndex, bool copyPrevious = true)
    {
        // Create new buffer, copy all read from this element to this, to avoid splitting Memory object
        // This leaves a bit of unused space in the previous buffer, but should only happen rarely, and the
        // performance is better this way, than the small amount of memory wasted.
        _buffers.Add(ArrayPool<char>.Shared.Rent(_buffers[_buffersIndex].Length + BufferSize));
        if (copyPrevious)
        {
            Array.Copy(
                _buffers[_buffersIndex],
                startIndex,
                _buffers[_buffersIndex + 1],
                0,
                _buffers[_buffersIndex].Length - startIndex);

            _index = _buffers[_buffersIndex].Length - startIndex;
        }

        _buffersIndex++;
    }

    private void ProcessElement()
    {
        CurrentNodeName = _xmlReader!.LocalName;
        CurrentNodeType = NodeType.StartElement;
        CanReadValue = false;

        if (_xmlReader.HasAttributes)
        {
            HasAttributes = true;
            return;
        }

        if (HandleEmptyElement()) return;

        _xmlReader.ReadStartElement();
        if (_xmlReader.NodeType != XmlNodeType.Text && _xmlReader.LocalName != CurrentNodeName)
            _hasReachedNewNode = true;

        if (_xmlReader!.NodeType == XmlNodeType.Text) CanReadValue = true;
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
        _hasReachedNewNode = false;
    }

    private bool HandleEmptyElement()
    {
        if (_xmlReader!.IsEmptyElement)
        {
            _emptyElementTag = _xmlReader.LocalName;
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureReader()
    {
        if (_xmlReader != null) return;
        var xmlReaderSettings = new XmlReaderSettings
        {
            Async = false,
            CheckCharacters = false,
            CloseInput = true,
            ConformanceLevel = ConformanceLevel.Auto,
            IgnoreWhitespace = true,
            IgnoreComments = true,
            ValidationType = ValidationType.None
        };

        _xmlReader = XmlReader.Create(_inputStream, xmlReaderSettings);
    }
}
