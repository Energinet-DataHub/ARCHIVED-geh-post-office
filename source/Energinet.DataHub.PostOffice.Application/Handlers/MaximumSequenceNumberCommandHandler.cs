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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Domain.Model;
using Energinet.DataHub.PostOffice.Domain.Repositories;
using MediatR;

namespace Energinet.DataHub.PostOffice.Application.Handlers
{
    public sealed class MaximumSequenceNumberCommandHandler :
        IRequestHandler<GetMaximumSequenceNumberCommand, long>,
        IRequestHandler<UpdateMaximumSequenceNumberCommand>
    {
        private readonly ISequenceNumberRepository _sequenceNumberRepository;

        public MaximumSequenceNumberCommandHandler(ISequenceNumberRepository sequenceNumberRepository)
        {
            _sequenceNumberRepository = sequenceNumberRepository;
        }

        public async Task<long> Handle(GetMaximumSequenceNumberCommand request, CancellationToken cancellationToken)
        {
            var number = await _sequenceNumberRepository
                .GetMaximumSequenceNumberAsync()
                .ConfigureAwait(false);

            await _sequenceNumberRepository
                .LogMaximumSequenceNumberAsync(number)
                .ConfigureAwait(false);

            return number.Value;
        }

        public async Task<Unit> Handle(UpdateMaximumSequenceNumberCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            await _sequenceNumberRepository
                .AdvanceSequenceNumberAsync(new SequenceNumber(request.SequenceNumber))
                .ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
