using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Energinet.DataHub.PostOffice.Common.SimpleInjector
{
    public sealed class SimpleInjectorScopedRequest : IFunctionsWorkerMiddleware
    {
        private readonly Container _container;

        public SimpleInjectorScopedRequest(Container container)
        {
            _container = container;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            await using var scope = AsyncScopedLifestyle.BeginScope(_container);
            if (scope.Container == null) throw new InvalidOperationException("Scope doesn't contain a container.");
            context.InstanceServices = new SimpleInjectorServiceProviderAdapter(scope.Container);
            await next(context).ConfigureAwait(false);
        }
    }
}
