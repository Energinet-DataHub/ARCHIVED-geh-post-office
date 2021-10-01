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
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Common.SimpleInjector;
using SimpleInjector;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.PostOffice.Tests.Common.SimpleInjector
{
    [UnitTest]
    public class SimpleInjectorScopedRequestTests
    {
        [Fact]
        public async Task Invoke_ContextIsNull_Throws()
        {
            // arrange
            await using var container = new Container();
            var target = new SimpleInjectorScopedRequest(container);

            // act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.Invoke(null!, _ => Task.CompletedTask)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Invoke_NextIsNull_Throws()
        {
            // arrange
            await using var container = new Container();
            var target = new SimpleInjectorScopedRequest(container);

            // act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.Invoke(new MockedFunctionContext(), null!)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Invoke_AllGood_NextCalled()
        {
            // arrange
            var nextCalled = false;
            await using var container = new Container();
            var target = new SimpleInjectorScopedRequest(container);

            // act, assert
            await target.Invoke(new MockedFunctionContext(), _ => Task.FromResult(nextCalled = true)).ConfigureAwait(false);

            // assert
            Assert.True(nextCalled);
        }
    }
}
