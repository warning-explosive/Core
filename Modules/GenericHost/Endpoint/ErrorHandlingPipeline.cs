namespace SpaceEngineers.Core.GenericHost.Endpoint
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Core.GenericEndpoint.Abstractions;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class ErrorHandlingPipeline : IMessagePipelineStep
    {
        private readonly IRetryPolicy _retryPolicy;

        public ErrorHandlingPipeline(
            IMessagePipeline decoratee,
            IRetryPolicy retryPolicy)
        {
            Decoratee = decoratee;
            _retryPolicy = retryPolicy;
        }

        public IMessagePipeline Decoratee { get; }

        public Task Process<TMessage>(TMessage message, IExtendedIntegrationContext context, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            return ExecutionExtensions
                .Try(async () => await Decoratee.Process(message, context, token).ConfigureAwait(false))
                .Invoke(ex => OnError(ex, message, context, token));
        }

        private Task OnError<TMessage>(Exception exception, TMessage message, IExtendedIntegrationContext context, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            // TODO: log error
            return _retryPolicy.Apply(message, context, token);
        }
    }
}