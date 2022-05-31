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
using System.Text.Json;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements.Containers;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Helpers;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Reader;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements.ValueElements;

internal class CimObjectWithCodingSchemeElement : ICimElement, IDisposable
{
    private readonly CimObjectElement _element;

    public CimObjectWithCodingSchemeElement(string key)
    {
        Key = key;
        _element = new CimObjectElement(key, 2);
    }

    public string Key { get; }

    public void WriteJson(Utf8JsonWriter jsonWriter)
    {
        ArgumentNullException.ThrowIfNull(jsonWriter, nameof(jsonWriter));
        _element.WriteJson(jsonWriter);
    }

    public void Update(CimXmlReader reader)
    {
        do
        {
            switch (reader.CurrentAttributeName)
            {
                case ElementNamesHelper.GenericElementNames.Attributes.CodingScheme:
                    _element.AddString(ElementNamesHelper.GenericElementNames.Attributes.CodingScheme, 0, reader);
                    break;
            }
        }
        while (reader.AdvanceAttribute());
        do
        {
            if (reader.CurrentNodeType != NodeType.StartElement) continue;
            _element.AddString(ElementNamesHelper.GenericElementNames.Value, 1, reader);
        }
        while (reader.AdvanceUntilClosed(Key));
    }

    public void Dispose()
    {
        _element.Dispose();
    }
}
