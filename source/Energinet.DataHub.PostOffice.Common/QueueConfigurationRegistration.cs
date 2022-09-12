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

using Energinet.DataHub.MessageHub.Core;
using Energinet.DataHub.PostOffice.Common.Configuration;
using Energinet.DataHub.PostOffice.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common
{
    internal static class QueueConfigurationRegistration
    {
        public static void AddQueueConfiguration(this Container container)
        {
            container.RegisterSingleton(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                return new PeekRequestConfig(
                    configuration.GetSetting(Settings.TimeSeriesQueue),
                    configuration.GetSetting(Settings.TimeSeriesReplyQueue),
                    configuration.GetSetting(Settings.ChargesQueue),
                    configuration.GetSetting(Settings.ChargesReplyQueue),
                    configuration.GetSetting(Settings.MarketRolesQueue),
                    configuration.GetSetting(Settings.MarketRolesReplyQueue),
                    configuration.GetSetting(Settings.MeteringPointsQueue),
                    configuration.GetSetting(Settings.MeteringPointsReplyQueue),
                    configuration.GetSetting(Settings.WholesaleQueue),
                    configuration.GetSetting(Settings.WholesaleReplyQueue));
            });

            container.RegisterSingleton(() =>
            {
                var configuration = container.GetService<IConfiguration>();
                return new DequeueConfig(
                    configuration.GetSetting(Settings.TimeSeriesDequeueQueue),
                    configuration.GetSetting(Settings.ChargesDequeueQueue),
                    configuration.GetSetting(Settings.MarketRolesDequeueQueue),
                    configuration.GetSetting(Settings.MeteringPointsDequeueQueue),
                    configuration.GetSetting(Settings.WholesaleDequeueQueue));
            });
        }
    }
}
