namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Messaging.MessageHeaders;

    /// <summary>
    /// ErrorHandlingMiddleware
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    public class ErrorHandlingMiddleware : IMessageHandlerMiddleware,
                                           ICollectionResolvable<IMessageHandlerMiddleware>
    {
        private readonly IEnumerable<IErrorHandler> _errorHandlers;

        /// <summary> .cctor </summary>
        /// <param name="errorHandlers">Error handlers</param>
        public ErrorHandlingMiddleware(IEnumerable<IErrorHandler> errorHandlers)
        {
            _errorHandlers = errorHandlers;
        }

        /// <inheritdoc />
        public async Task Handle(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token)
        {
            await next(context, token)
               .TryAsync()
               .Catch<Exception>(OnError(context))
               .Invoke(token)
               .ConfigureAwait(false);
        }

        private Func<Exception, CancellationToken, Task> OnError(
            IAdvancedIntegrationContext context)
        {
            return (exception, token) => InvokeErrorHandlers(context, exception, _errorHandlers, token)
               .TryAsync()
               .Catch<Exception>(OnErrorHandlingError(context))
               .Invoke(token);
        }

        private static async Task InvokeErrorHandlers(
            IAdvancedIntegrationContext context,
            Exception exception,
            IEnumerable<IErrorHandler> errorHandlers,
            CancellationToken token)
        {
            foreach (var handler in errorHandlers)
            {
                await handler
                   .Handle(context, exception, token)
                   .ConfigureAwait(false);
            }
        }

        private static Func<Exception, CancellationToken, Task> OnErrorHandlingError(
            IAdvancedIntegrationContext context)
        {
            return (exception, _) =>
            {
                context.Message.WriteHeader(new RejectReason(exception));
                return Task.CompletedTask;
            };
        }
    }
}