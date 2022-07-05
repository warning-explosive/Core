namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Collections.Generic;
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
        private readonly IEnumerable<IErrorHandler> _errorHandlers;

        public ErrorHandlingPipeline(
            IMessagePipeline decoratee,
            IEnumerable<IErrorHandler> errorHandlers)
        {
            Decoratee = decoratee;

            _errorHandlers = errorHandlers;
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

        private Func<Exception, CancellationToken, Task<Exception?>> OnError(
            IAdvancedIntegrationContext context)
        {
            return async (exception, token) =>
            {
                var errorHandlingException = await ExecutionExtensions
                   .TryAsync((context, _errorHandlers, exception), InvokeErrorHandlers)
                   .Catch<Exception>()
                   .Invoke(OnErrorHandlingError, token)
                   .ConfigureAwait(false);

                return errorHandlingException ?? exception;
            };
        }

        private static async Task<Exception?> InvokeErrorHandlers(
            (IAdvancedIntegrationContext, IEnumerable<IErrorHandler>, Exception) state,
            CancellationToken token)
        {
            var (context, errorHandlers, exception) = state;

            foreach (var handler in errorHandlers)
            {
                await handler
                   .Handle(context, exception, token)
                   .ConfigureAwait(false);
            }

            return default;
        }

        private static Task<Exception?> OnErrorHandlingError(
            Exception exception,
            CancellationToken token)
        {
            return Task.FromResult<Exception?>(exception);
        }
    }
}