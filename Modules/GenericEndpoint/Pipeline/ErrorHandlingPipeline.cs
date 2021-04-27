namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using Microsoft.Extensions.Logging;

    [Component(EnLifestyle.Singleton)]
    [Dependency(typeof(UnitOfWorkPipeline))]
    internal class ErrorHandlingPipeline : IMessagePipelineStep, IMessagePipeline
    {
        private readonly IRetryPolicy _retryPolicy;
        private readonly ILogger _logger;

        public ErrorHandlingPipeline(
            IMessagePipeline decoratee,
            IRetryPolicy retryPolicy,
            ILogger logger)
        {
            Decoratee = decoratee;

            _retryPolicy = retryPolicy;
            _logger = logger;
        }

        public IMessagePipeline Decoratee { get; }

        public Task Process(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(() => Decoratee.Process(context, token))
                .Invoke(ex => OnError(context, ex, token));
        }

        private Task OnError(IAdvancedIntegrationContext context, Exception exception, CancellationToken token)
        {
            _logger.Error(exception);
            return _retryPolicy.Apply(context, exception, token);
        }
    }
}