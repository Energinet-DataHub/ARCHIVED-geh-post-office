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

namespace TestJSonConversion.SimpleCimJson.Reader;

/// <summary>
/// Reads nodes from an XML stream
/// </summary>
internal interface ICimXmlReader
{
    /// <summary>
    /// The name of the current node
    /// </summary>
    string CurrentNodeName { get; }

    /// <summary>
    /// The name of the current attribute
    /// </summary>
    string CurrentAttributeName { get; }

    /// <summary>
    /// If we can read a value from the current node
    /// </summary>
    bool CanReadValue { get; }

    /// <summary>
    /// The type <see cref="NodeType"/> of node we are currently at
    /// </summary>
    NodeType CurrentNodeType { get; }

    /// <summary>
    /// Does the current node have Attributes
    /// </summary>
    bool HasAttributes { get; }

    /// <summary>
    /// Advances to the next node in the XML
    /// </summary>
    /// <returns>true if we advanced to the next node, false if at end of file</returns>
    bool Advance();

    /// <summary>
    /// Advances to the next attribute on the current element
    /// </summary>
    /// <returns>true if we advanced to the next attribute, false if no more attributes on node</returns>
    /// <remarks>When the last attribute is read, it will reset the reader to the element that contained the attributes</remarks>
    bool AdvanceAttribute();

    /// <summary>
    /// Reads the current nodes value as a string
    /// </summary>
    /// <returns>The value of the current node as a string</returns>
    ReadOnlyMemory<char> ReadValue();
}