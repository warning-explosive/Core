namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;

    [Component(EnLifestyle.Singleton)]
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

        public async Task Process(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> producer,
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            var exception = await ExecutionExtensions
               .TryAsync((producer, context), Process)
               .Catch<Exception>()
               .Invoke(OnError(context), token)
               .ConfigureAwait(false);

            if (exception == null)
            {
                await context
                   .Accept(token)
                   .ConfigureAwait(false);
            }
        }

        private async Task<Exception?> Process(
            (Func<IAdvancedIntegrationContext, CancellationToken, Task>, IAdvancedIntegrationContext) state,
            CancellationToken token)
        {
            var (producer, context) = state;

            await Decoratee
               .Process(producer, context, token)
               .ConfigureAwait(false);

            return default;
        }

        private Func<Exception, CancellationToken, Task<Exception?>> OnError(IAdvancedIntegrationContext context)
        {
            return async (exception, token) =>
            {
                var retryException = await ExecutionExtensions
                   .TryAsync((context, _retryPolicy, exception), Retry)
                   .Catch<Exception>()
                   .Invoke(OnRetryError, token)
                   .ConfigureAwait(false);

                return retryException ?? exception;
            };
        }

        private static async Task<Exception?> Retry(
            (IAdvancedIntegrationContext, IRetryPolicy, Exception) state,
            CancellationToken token)
        {
            var (context, policy, exception) = state;

            await policy
               .Apply(context, exception, token)
               .ConfigureAwait(false);

            return default;
        }

        private static Task<Exception?> OnRetryError(Exception exception, CancellationToken token)
        {
            return Task.FromResult<Exception?>(exception);
        }
    }
}