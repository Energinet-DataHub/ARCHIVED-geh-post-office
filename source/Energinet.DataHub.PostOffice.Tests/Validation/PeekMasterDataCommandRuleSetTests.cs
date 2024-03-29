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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Validation;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Validation
{
    [UnitTest]
    public sealed class PeekMasterDataCommandRuleSetTests
    {
        private const ResponseFormat ResponseFormat = MessageHub.Model.Model.ResponseFormat.Json;
        private const double ResponseVersion = 1.0;
        private const string ValidRecipient = "7ED1FED8-E6E9-4055-9136-08C706EE1830";

        [Theory]
        [InlineData("", false)]
        [InlineData(null, true)]
        [InlineData("  ", false)]
        [InlineData("8F9B8218-BAE6-412B-B91B-0C78A55FF128", true)]
        [InlineData("8F9B8218-BAE6-412B-B91B-0C78A55FF1XX", false)]
        public async Task Validate_BundleId_ValidatesProperty(string value, bool isValid)
        {
            // Arrange
            const string propertyName = nameof(PeekMasterDataCommand.BundleId);

            var target = new PeekMasterDataCommandRuleSet();
            var command = new PeekMasterDataCommand(
                ValidRecipient,
                value,
                ResponseFormat,
                ResponseVersion);

            // Act
            var result = await target.ValidateAsync(command).ConfigureAwait(false);

            // Assert
            if (isValid)
            {
                Assert.True(result.IsValid);
                Assert.DoesNotContain(propertyName, result.Errors.Select(x => x.PropertyName));
            }
            else
            {
                Assert.False(result.IsValid);
                Assert.Contains(propertyName, result.Errors.Select(x => x.PropertyName));
            }
        }

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("  ", false)]
        [InlineData("8B7B8949-4104-4F14-BD81-5C01FA095B99", true)]
        public async Task Validate_Recipient_ValidatesProperty(string value, bool isValid)
        {
            // Arrange
            const string propertyName = nameof(PeekMasterDataCommand.MarketOperator);

            var target = new PeekMasterDataCommandRuleSet();
            var command = new PeekMasterDataCommand(
                value,
                Guid.NewGuid().ToString(),
                ResponseFormat,
                ResponseVersion);

            // Act
            var result = await target.ValidateAsync(command).ConfigureAwait(false);

            // Assert
            if (isValid)
            {
                Assert.True(result.IsValid);
                Assert.DoesNotContain(propertyName, result.Errors.Select(x => x.PropertyName));
            }
            else
            {
                Assert.False(result.IsValid);
                Assert.Contains(propertyName, result.Errors.Select(x => x.PropertyName));
            }
        }
    }
}
