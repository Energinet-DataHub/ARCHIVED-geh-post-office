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

using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Application.Commands;
using Energinet.DataHub.PostOffice.Application.Validation;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Validation;

[UnitTest]
public sealed class UpdateActorCommandRuleSetTests
{
    private const string ValidUuid = "169B53A2-0A17-47D7-9603-4E41854E4181";

    [Theory]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("  ", false)]
    [InlineData("8F9B8218-BAE6-412B-B91B-0C78A55FF128", true)]
    [InlineData("8F9B8218-BAE6-412B-B91B-0C78A55FF1XX", false)]
    public async Task Validate_ActorId_ValidatesProperty(string value, bool isValid)
    {
        // Arrange
        const string propertyName = nameof(UpdateActorCommand.ActorId);

        var target = new UpdateActorCommandRuleSet();
        var command = new UpdateActorCommand(
            value,
            ValidUuid);

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
    [InlineData("8F9B8218-BAE6-412B-B91B-0C78A55FF128", true)]
    [InlineData("8F9B8218-BAE6-412B-B91B-0C78A55FF1XX", false)]
    public async Task Validate_ExternalActorId_ValidatesProperty(string value, bool isValid)
    {
        // Arrange
        const string propertyName = nameof(UpdateActorCommand.ExternalActorId);

        var target = new UpdateActorCommandRuleSet();
        var command = new UpdateActorCommand(
            ValidUuid,
            value);

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
