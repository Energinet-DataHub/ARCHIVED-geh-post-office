using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Energinet.DataHub.PostOffice.Outbound.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.PostOffice.Outbound.Functions
{
    public sealed class Dequeue
    {
        private readonly IMediator _mediator;

        public Dequeue(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("Dequeue")]
        public async Task<HttpResponseMessage> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "delete")]
            HttpRequestData request,
            FunctionContext context)
        {
            var logger = context.GetLogger<Dequeue>();
            var command = request.GetDequeueCommand();

            logger.LogInformation($"Processing Dequeue query: {command}.");

            await _mediator.Send(command).ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
