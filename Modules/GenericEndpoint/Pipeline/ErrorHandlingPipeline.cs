namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Singleton)]
    [Dependency(typeof(QueryReplyValidationPipeline))]
    internal class ErrorHandlingPipeline : IMessagePipelineStep, IMessagePipeline
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

        public Task Process(IntegrationMessage message, IExtendedIntegrationContext context, CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(() => Decoratee.Process(message, context, token))
                .Invoke(_ => OnError(message, context, token));
        }

        private Task OnError(IntegrationMessage message, IExtendedIntegrationContext context, CancellationToken token)
        {
            // TODO: log error
            return _retryPolicy.Apply(message, context, token);
        }
    }
}