namespace SpaceEngineers.Core.GenericEndpoint.Tracing.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contract;
    using CrossCuttingConcerns.Json;
    using GenericEndpoint.Pipeline;
    using Messaging.Abstractions;
    using TracingEndpoint.Contract;
    using TracingEndpoint.Contract.Messages;

    internal static class TracingExtensions
    {
        internal static async Task CaptureTrace(
            this IAdvancedIntegrationContext context,
            EndpointIdentity endpointIdentity,
            Exception? exception,
            IIntegrationMessageFactory integrationMessageFactory,
            IJsonSerializer jsonSerializer,
            CancellationToken token)
        {
            if (context.Message.Payload is not TracingEndpoint.Contract.Messages.CaptureTrace)
            {
                var command = new CaptureTrace(
                    SerializedIntegrationMessage.FromIntegrationMessage(context.Message, jsonSerializer),
                    exception);

                var message = integrationMessageFactory.CreateGeneralMessage(
                    command,
                    endpointIdentity,
                    context.Message);

                var wasSent = await context
                   .SendMessage(message, token)
                   .ConfigureAwait(false);

                if (!wasSent)
                {
                    throw new InvalidOperationException("Trace wasn't captured");
                }
            }
        }
    }
}