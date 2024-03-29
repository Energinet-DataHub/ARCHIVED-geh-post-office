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

using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Model;

namespace Energinet.DataHub.MessageHub.Model.Peek
{
    /// <summary>
    /// Parses the response for the bundle content request sent from a sub-domain.
    /// </summary>
    public interface IResponseBundleParser
    {
        /// <summary>
        /// Converts the specified response into a protobuf contract.
        /// </summary>
        /// <param name="dataBundleResponseDto">The response to convert.</param>
        /// <returns>A byte array with the parsed RequestDataBundleResponseDto</returns>
        byte[] Parse(DataBundleResponseDto dataBundleResponseDto);

        /// <summary>
        /// Parses the protobuf contract response.
        /// </summary>
        /// <param name="dataBundleReplyContract">The bytes containing the protobuf contract.</param>
        /// <exception cref="MessageHubException">
        /// Throws an exception if byte array cannot be parsed.
        /// </exception>
        /// <returns><see cref="DataBundleResponseDto"/>Returns a dto with the object</returns>
        DataBundleResponseDto Parse(byte[] dataBundleReplyContract);
    }
}
