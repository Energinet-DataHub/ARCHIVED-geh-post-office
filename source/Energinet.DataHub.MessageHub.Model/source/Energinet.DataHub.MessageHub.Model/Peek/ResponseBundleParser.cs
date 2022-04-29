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
    public sealed class ResponseBundleParser : IResponseBundleParser
    {
        public byte[] Parse(DataBundleResponseDto dataBundleResponseDto)
        {
            ArgumentNullException.ThrowIfNull(dataBundleResponseDto, nameof(dataBundleResponseDto));

            var contract = new DataBundleResponseContract
            {
                RequestId = dataBundleResponseDto.RequestId.ToString()
            };

            if (dataBundleResponseDto.IsErrorResponse)
            {
                var contractErrorReason = MapToFailureReason(dataBundleResponseDto.ResponseError.Reason);
                contract.Failure = new DataBundleResponseContract.Types.RequestFailure
                {
                    Reason = contractErrorReason,
                    FailureDescription = dataBundleResponseDto.ResponseError.FailureDescription
                };
            }
            else
            {
                contract.Success = new DataBundleResponseContract.Types.FileResource
                {
                    ContentUri = dataBundleResponseDto.ContentUri.AbsoluteUri
                };
            }

            return contract.ToByteArray();
        }

        public DataBundleResponseDto Parse(byte[] dataBundleReplyContract)
        {
            try
            {
                var bundleResponse = DataBundleResponseContract.Parser.ParseFrom(dataBundleReplyContract);
                var requestId = Guid.Parse(bundleResponse.RequestId);
                var requestIdempotency = bundleResponse.RequestIdempotencyId;

                if (bundleResponse.ReplyCase == DataBundleResponseContract.ReplyOneofCase.Success)
                {
                    var successReply = bundleResponse.Success;
                    return new DataBundleResponseDto(
                        requestId,
                        requestIdempotency,
                        new Uri(successReply.ContentUri));
                }

                var failureReply = bundleResponse.Failure;
                var errorResponse = new DataBundleResponseErrorDto(
                    MapToFailureReason(failureReply.Reason),
                    failureReply.FailureDescription);

                return new DataBundleResponseDto(
                    requestId,
                    requestIdempotency,
                    errorResponse);
            }
            catch (Exception ex) when (ex is InvalidProtocolBufferException or FormatException)
            {
                throw new MessageHubException("Error parsing bytes for DataBundleRequestDto", ex);
            }
        }

        private static DataBundleResponseContract.Types.RequestFailure.Types.Reason MapToFailureReason(DataBundleResponseErrorReason errorReason)
        {
            return errorReason switch
            {
                DataBundleResponseErrorReason.DatasetNotFound => DataBundleResponseContract.Types.RequestFailure.Types.Reason.DatasetNotFound,
                DataBundleResponseErrorReason.DatasetNotAvailable => DataBundleResponseContract.Types.RequestFailure.Types.Reason.DatasetNotAvailable,
                DataBundleResponseErrorReason.InternalError => DataBundleResponseContract.Types.RequestFailure.Types.Reason.InternalError,
                _ => DataBundleResponseContract.Types.RequestFailure.Types.Reason.InternalError
            };
        }

        private static DataBundleResponseErrorReason MapToFailureReason(DataBundleResponseContract.Types.RequestFailure.Types.Reason errorReason)
        {
            return errorReason switch
            {
                DataBundleResponseContract.Types.RequestFailure.Types.Reason.DatasetNotFound => DataBundleResponseErrorReason.DatasetNotFound,
                DataBundleResponseContract.Types.RequestFailure.Types.Reason.DatasetNotAvailable => DataBundleResponseErrorReason.DatasetNotAvailable,
                DataBundleResponseContract.Types.RequestFailure.Types.Reason.InternalError => DataBundleResponseErrorReason.InternalError,
                _ => DataBundleResponseErrorReason.InternalError
            };
        }
    }
}
