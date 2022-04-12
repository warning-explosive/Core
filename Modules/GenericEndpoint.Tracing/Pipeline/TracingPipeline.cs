namespace SpaceEngineers.Core.GenericEndpoint.Tracing.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using CrossCuttingConcerns.Json;
    using GenericEndpoint.Pipeline;
    using TracingEndpoint.Contract;
    using TracingEndpoint.Contract.Messages;

    [Component(EnLifestyle.Singleton)]
    [Dependency(typeof(GenericEndpoint.Pipeline.QueryReplyValidationPipeline))]
    [Dependent(typeof(GenericEndpoint.Pipeline.UnitOfWorkPipeline))]
    internal class TracingPipeline : IMessagePipelineStep, IMessagePipeline
    {
        private readonly IJsonSerializer _jsonSerializer;

        public TracingPipeline(
            IMessagePipeline decoratee,
            IJsonSerializer jsonSerializer)
        {
            Decoratee = decoratee;

            _jsonSerializer = jsonSerializer;
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

        private Task OnSuccess(IAdvancedIntegrationContext context, CancellationToken token)
        {
            if (context.Message.Payload is not CaptureTrace)
            {
                var command = new CaptureTrace(SerializedIntegrationMessage.FromIntegrationMessage(context.Message, _jsonSerializer), null);
                return context.Send(command, token);
            }

            return Task.CompletedTask;
        }

        private Func<Exception, CancellationToken, Task> OnError(IAdvancedIntegrationContext context)
        {
            return async (exception, token) =>
            {
                if (context.Message.Payload is not CaptureTrace)
                {
                    var command = new CaptureTrace(SerializedIntegrationMessage.FromIntegrationMessage(context.Message, _jsonSerializer), exception);
                    await context.Send(command, token).ConfigureAwait(false);
                }

                throw exception.Rethrow();
            };
        }
    }
}