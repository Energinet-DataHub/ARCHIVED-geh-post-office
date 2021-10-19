using System;
using System.Linq;
using Energinet.DataHub.MessageHub.Client.Exceptions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageHub.Client.Tests.Exceptions
{
    [UnitTest]
    public class ExceptionTests
    {
        [Fact]
        public void Ctor_Exists_IsCalledSuccessfully()
        {
            // arrange
            var expectedCount = typeof(MessageHubException).Assembly.GetTypes()
                .Count(x => x.IsAssignableTo(typeof(Exception))) * 3;

            var exceptionCtors = new Action[]
            {
#pragma warning disable CA1806
                // ReSharper disable ObjectCreationAsStatement
                () => new MessageHubException(),
                () => new MessageHubException("message"),
                () => new MessageHubException("message", new InvalidOperationException()),
                () => new MessageHubStorageException(),
                () => new MessageHubStorageException("message"),
                () => new MessageHubStorageException("message", new InvalidOperationException()),
                // ReSharper restore ObjectCreationAsStatement
#pragma warning restore CA1806
            };

            // act
            foreach (var ctor in exceptionCtors)
            {
                ctor();
            }

            // assert
            Assert.True(
                expectedCount == exceptionCtors.Length,
                "Expected number of invoked exception constructos differs from actual count. Have new exception types been added? :)");
        }
    }
}
