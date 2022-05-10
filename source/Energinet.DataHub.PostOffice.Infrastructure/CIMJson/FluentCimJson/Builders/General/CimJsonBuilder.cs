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
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Elements;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.Descriptor;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Interfaces.General;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Reader;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Builders.General
{
    internal class CimJsonBuilder : ICimJsonAddXmlDataSource
    {
        private readonly List<ICimJsonElementDescriptor> _elementDescriptors;
        private CimXmlReader? _xmlReader;
        private CimJsonBuilder()
        {
            _elementDescriptors = new List<ICimJsonElementDescriptor>(20);
        }

        public static ICimJsonAddXmlDataSource Create() => new CimJsonBuilder();

        public CimJsonBuilder WithXmlReader(Action<ICimJsonConfigureElementDescriptor> configure, CimXmlReader reader)
        {
            var builder = new CimJsonElementDescriptorBuilder();
            configure(builder);
            _elementDescriptors.AddRange(builder.BuildDescriptor());
            _xmlReader = reader;
            return this;
        }

        public async ValueTask BuildAsync(Utf8JsonWriter jsonWriter)
        {
            if (_xmlReader is not null)
            {
                var elementsToWrite = new List<ICimJsonElement>(_elementDescriptors.Count);
                foreach (var elementDescriptor in _elementDescriptors)
                {
                    var element = elementDescriptor.CreateElement();
                    if (await ReadToElementAsync(element.Name, element.IsOptional).ConfigureAwait(false))
                    {
                        element.ReadData(_xmlReader);
                        elementsToWrite.Add(element);
                    }
                }

                elementsToWrite.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
                foreach (var jsonElement in elementsToWrite)
                {
                    jsonElement.WriteJson(jsonWriter);
                    jsonElement.ReturnElementToPool();
                }
            }
            else
            {
                throw new InvalidOperationException(
                    "XmlReader was not correctly initialized or is null, we can't read an XML file without a valid reader");
            }
        }

        private async Task<bool> ReadToElementAsync(string elementName, bool isOptional)
        {
            if (_xmlReader is null) return false;
            if (_xmlReader.CurrentNodeName == elementName && _xmlReader.CurrentNodeType == NodeType.StartElement)
                return true;

            if (_xmlReader.CurrentNodeType == NodeType.EndElement)
                await _xmlReader.AdvanceAsync().ConfigureAwait(false);

            while (await _xmlReader.AdvanceAsync().ConfigureAwait(false))
            {
                if (_xmlReader.CurrentNodeType == NodeType.StartElement && _xmlReader.CurrentNodeName == elementName)
                    return true;

                if (_xmlReader.CurrentNodeType == NodeType.StartElement && _xmlReader.CurrentNodeName != elementName && isOptional)
                    return false;
            }

            return false;
            // if (_xmlReader is null) return false;
            // if (_xmlReader.IsStartElement() && _xmlReader.LocalName == elementName)
            // {
            //     return true;
            // }
            //
            // if (isOptional && _xmlReader.IsStartElement() && _xmlReader.LocalName != elementName)
            //     return false;
            //
            // while (_xmlReader.Read())
            // {
            //     _xmlReader.MoveToContent();
            //     if (_xmlReader.NodeType == XmlNodeType.Element && _xmlReader.LocalName == elementName)
            //     {
            //         return true;
            //     }
            //
            //     if (isOptional && _xmlReader.IsStartElement() && _xmlReader.LocalName != elementName)
            //         return false;
            // }
            //
            // return false;
        }
    }
}
