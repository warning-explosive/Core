namespace SpaceEngineers.Core.GenericEndpoint.Tracing.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using Contract;
    using CrossCuttingConcerns.Json;
    using GenericEndpoint.Pipeline;
    using Messaging.Abstractions;
    using TracingEndpoint.Contract;
    using TracingEndpoint.Contract.Messages;

    [Component(EnLifestyle.Singleton)]
    [Dependency(typeof(GenericEndpoint.Pipeline.UnitOfWorkPipeline))]
    [Dependent(typeof(GenericEndpoint.Pipeline.ErrorHandlingPipeline))]
    internal class TracingPipeline : IMessagePipelineStep, IMessagePipeline
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IIntegrationMessageFactory _integrationMessageFactory;

        public TracingPipeline(
            IMessagePipeline decoratee,
            EndpointIdentity endpointIdentity,
            IJsonSerializer jsonSerializer,
            IIntegrationMessageFactory integrationMessageFactory)
        {
            Decoratee = decoratee;

            _endpointIdentity = endpointIdentity;
            _jsonSerializer = jsonSerializer;
            _integrationMessageFactory = integrationMessageFactory;
        }

        public IMessagePipeline Decoratee { get; }

        public Task Process(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> producer,
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync((producer, context), Process)
                .Catch<Exception>(OnError(context))
                .Invoke(token);
        }

        private async Task Process(
            (Func<IAdvancedIntegrationContext, CancellationToken, Task>, IAdvancedIntegrationContext) state,
            CancellationToken token)
        {
            var (producer, context) = state;
            await Decoratee.Process(producer, context, token).ConfigureAwait(false);
            await OnSuccess(context, token).ConfigureAwait(false);
        }

        private async Task OnSuccess(IAdvancedIntegrationContext context, CancellationToken token)
        {
            await CaptureTrace(context, default, token).ConfigureAwait(false);
        }

        private Func<Exception, CancellationToken, Task> OnError(IAdvancedIntegrationContext context)
        {
            return async (exception, token) =>
            {
                await CaptureTrace(context, exception, token).ConfigureAwait(false);

                throw exception.Rethrow();
            };
        }

        private async Task CaptureTrace(
            IAdvancedIntegrationContext context,
            Exception? exception,
            CancellationToken token)
        {
            if (context.Message.Payload is not TracingEndpoint.Contract.Messages.CaptureTrace)
            {
                var command = new CaptureTrace(
                    SerializedIntegrationMessage.FromIntegrationMessage(context.Message, _jsonSerializer),
                    exception);

                var message = _integrationMessageFactory.CreateGeneralMessage(
                    command,
                    _endpointIdentity,
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