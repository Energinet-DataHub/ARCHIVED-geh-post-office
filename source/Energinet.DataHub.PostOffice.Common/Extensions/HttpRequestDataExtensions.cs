﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Common.Extensions
{
    public static class HttpRequestDataExtensions
    {
        public static HttpResponseData CreateResponse(this HttpRequestData source, Stream stream, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var response = source.CreateResponse(statusCode);
            response.Body = stream;

            return response;
        }

        public static async Task<HttpResponseData> ProcessAsync(this HttpRequestData request, Func<Task<HttpResponseData>> worker, [CallerFilePath] string? callerFilePath = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (worker == null)
                throw new ArgumentNullException(nameof(worker));

            var callerClass = Path.GetFileNameWithoutExtension(callerFilePath)!;
            var logger = request.FunctionContext.GetLogger(callerClass);

            try
            {
                logger.Log(LogLevel.Information, $"Processing {callerClass}");
                return await worker().ConfigureAwait(false);
            }
#pragma warning disable CA1031
            catch (Exception e)
#pragma warning restore CA1031
            {
                logger.LogError(e, "An error occurred while processing request");
                return e switch
                {
                    ValidationException => request.CreateResponse($"A validation error occurred while processing {callerClass}: {e.Message}", HttpStatusCode.BadRequest),
                    _ => request.CreateResponse($"An error occured while processing {callerClass}", HttpStatusCode.InternalServerError)
                };
            }
        }

        private static HttpResponseData CreateResponse(this HttpRequestData source, string message, HttpStatusCode statusCode)
        {
            return source.CreateResponse(new MemoryStream(Encoding.UTF8.GetBytes(message)), statusCode);
        }
    }
}
