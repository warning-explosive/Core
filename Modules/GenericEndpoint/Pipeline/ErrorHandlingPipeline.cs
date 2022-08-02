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
    using Messaging.MessageHeaders;

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
            await ExecutionExtensions
               .TryAsync((producer, context), Process)
               .Catch<Exception>(OnError(context))
               .Invoke(token)
               .ConfigureAwait(false);
        }

        private Task Process(
            (Func<IAdvancedIntegrationContext, CancellationToken, Task>, IAdvancedIntegrationContext) state,
            CancellationToken token)
        {
            var (producer, context) = state;

            return Decoratee.Process(producer, context, token);
        }

        private Func<Exception, CancellationToken, Task> OnError(
            IAdvancedIntegrationContext context)
        {
            return (exception, token) => ExecutionExtensions
               .TryAsync((context, exception, _errorHandlers), InvokeErrorHandlers)
               .Catch<Exception>(OnErrorHandlingError(context))
               .Invoke(token);
        }

        private static async Task InvokeErrorHandlers(
            (IAdvancedIntegrationContext, Exception, IEnumerable<IErrorHandler>) state,
            CancellationToken token)
        {
            var (context, exception, errorHandlers) = state;

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