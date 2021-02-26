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
    [Dependency(typeof(UnitOfWorkPipeline))]
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

        public Task Process(IExtendedIntegrationContext context, CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(() => Decoratee.Process(context, token))
                .Invoke(_ => OnError(context, token));
        }

        private Task OnError(IExtendedIntegrationContext context, CancellationToken token)
        {
            // TODO: log error
            return _retryPolicy.Apply(context, token);
        }
    }
}