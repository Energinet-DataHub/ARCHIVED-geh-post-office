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

using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.PostOffice.Application.GetMessage.Interfaces;

namespace Energinet.DataHub.PostOffice.Infrastructure.ContentPath
{
    public class GetContentPathStrategyFactory : IGetContentPathStrategyFactory
    {
        private readonly IEnumerable<IGetContentPathStrategy> _strategies;

        public GetContentPathStrategyFactory(IEnumerable<IGetContentPathStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IGetContentPathStrategy Create(string pathToExistingContent)
        {
            IGetContentPathStrategy strategy = string.IsNullOrWhiteSpace(pathToExistingContent)
                ? _strategies.FirstOrDefault(contentPathStrategy => contentPathStrategy.StrategyName.Equals(nameof(ContentPathFromSubDomain)))!
                : _strategies.FirstOrDefault(contentPathStrategy => contentPathStrategy.StrategyName.Equals(nameof(ContentPathFromSavedResponse)))!;

            strategy.SavedContentPath = pathToExistingContent;
            return strategy;
        }
    }
}
