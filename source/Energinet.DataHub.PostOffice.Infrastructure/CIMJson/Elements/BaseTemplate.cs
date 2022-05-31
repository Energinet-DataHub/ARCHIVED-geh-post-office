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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Reader;
using Microsoft.IO;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Elements;

internal abstract class BaseTemplate : IDisposable
{
    private readonly MemoryStream _jsonStream;
    private bool _isDisposed;

    protected BaseTemplate(RecyclableMemoryStreamManager manager, Stream xmlData)
    {
        _jsonStream = manager.GetStream();
        CimReader = new CimXmlReader(xmlData);

        JsonWriter = new Utf8JsonWriter(_jsonStream, new JsonWriterOptions
        {
            Indented = false,
            SkipValidation = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    protected Utf8JsonWriter JsonWriter { get; private set; }
    protected CimXmlReader CimReader { get; private set; }

    public void Dispose()
    {
        Dispose(true);
    }

    internal MemoryStream ConvertXmlToJson(Stream xmlData)
    {
        Convert();
        JsonWriter.Flush();
        return _jsonStream;
    }

    protected abstract void Convert();

    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            // free managed resources
            CimReader?.Dispose();
            JsonWriter?.Dispose();
            _jsonStream.Dispose();
        }

        _isDisposed = true;
    }
}
