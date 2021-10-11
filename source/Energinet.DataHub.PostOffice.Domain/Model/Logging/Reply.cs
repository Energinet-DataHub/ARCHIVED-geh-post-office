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

using Energinet.DataHub.MessageHub.Client.Model;

namespace Energinet.DataHub.PostOffice.Domain.Model.Logging
{
    public sealed class Reply
    {
        /// <summary>
        /// Creates a successful response with a reference to the returned bundle.
        /// </summary>
        /// <param name="bundleReference"></param>
        public Reply(IBundleContent bundleReference)
        {
            BundleReference = bundleReference;
        }

        /// <summary>
        /// Creates a failure response with reason and description of failure.
        /// </summary>
        /// <param name="bundleError"></param>
        public Reply(DataBundleResponseErrorDto bundleError)
        {
            BundleError = bundleError;
        }

        public IBundleContent? BundleReference { get; }
        public DataBundleResponseErrorDto? BundleError { get; }
    }
}
