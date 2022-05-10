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

using System.Diagnostics.CodeAnalysis;

namespace Energinet.DataHub.PostOffice.Domain.Model
{
    public sealed record ProcessId
    {
        private readonly string _processId;

        public ProcessId([NotNull] Uuid bundleId, [NotNull] ActorId recipient)
        {
            BundleId = bundleId;
            Recipient = recipient;
            _processId = string.Join("_", bundleId.ToString(), recipient.Value);
        }

        public Uuid BundleId { get; }

        public ActorId Recipient { get; }

        public override string ToString()
        {
            return _processId;
        }
    }
}
