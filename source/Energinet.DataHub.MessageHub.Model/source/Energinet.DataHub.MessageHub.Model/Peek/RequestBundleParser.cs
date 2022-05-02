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
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Protobuf;
using Google.Protobuf;

namespace Energinet.DataHub.MessageHub.Model.Peek
{
    public sealed class RequestBundleParser : IRequestBundleParser
    {
        public byte[] Parse(DataBundleRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var message = new DataBundleRequestContract
            {
                RequestId = request.RequestId.ToString(),
                DataAvailableNotificationReferenceId = request.DataAvailableNotificationReferenceId,
                IdempotencyId = request.IdempotencyId,
                MessageType = request.MessageType,
            };

            return message.ToByteArray();
        }

        public DataBundleRequestDto Parse(byte[] dataBundleRequestContract)
        {
            try
            {
                var bundleResponse = DataBundleRequestContract.Parser.ParseFrom(dataBundleRequestContract);
                return new DataBundleRequestDto(
                    Guid.Parse(bundleResponse.RequestId),
                    bundleResponse.DataAvailableNotificationReferenceId,
                    bundleResponse.IdempotencyId,
                    bundleResponse.MessageType);
            }
            catch (Exception ex) when (ex is InvalidProtocolBufferException or FormatException)
            {
                throw new MessageHubException("Error parsing bytes for DataBundleRequestDto", ex);
            }
        }
    }
}
