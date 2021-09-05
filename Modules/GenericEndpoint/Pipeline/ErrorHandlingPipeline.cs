namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
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

        private Task Process(
            (Func<IAdvancedIntegrationContext, CancellationToken, Task>, IAdvancedIntegrationContext) state,
            CancellationToken token)
        {
            var (producer, context) = state;
            return Decoratee.Process(producer, context, token);
        }

        private Func<Exception, CancellationToken, Task> OnError(IAdvancedIntegrationContext context)
        {
            return (exception, token) =>
            {
                _logger.Error(exception);
                return _retryPolicy.Apply(context, exception, token);
            };
        }
    }
}