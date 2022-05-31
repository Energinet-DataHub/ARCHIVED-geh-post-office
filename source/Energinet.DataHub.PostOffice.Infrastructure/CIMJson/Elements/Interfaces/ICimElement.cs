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

using System.Text.Json;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements;

/// <summary>
/// An element to describe a cim converted xml element in json
/// </summary>
public interface ICimElement
{
    /// <summary>
    /// The key of the current element
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Used to write the json that corresponds to this element
    /// </summary>
    /// <param name="jsonWriter"></param>
    void WriteJson(Utf8JsonWriter jsonWriter);
}
