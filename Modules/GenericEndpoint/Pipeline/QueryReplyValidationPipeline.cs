namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using Messaging;
    using Messaging.MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    [Dependent(typeof(UnitOfWorkPipeline))]
    internal class QueryReplyValidationPipeline : IMessagePipelineStep, IMessagePipeline
    {
        public QueryReplyValidationPipeline(IMessagePipeline decoratee)
        {
            Decoratee = decoratee;
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

            if (context.Message.IsQuery()
             && context.Message.ReadHeader<DidHandlerReplyToTheQuery>()?.Value != true)
            {
                throw new InvalidOperationException("Message handler should reply to the query");
            }
        }

        private static Func<Exception, CancellationToken, Task> OnError(IAdvancedIntegrationContext context)
        {
            return (_, _) =>
            {
                _ = context.Message.TryDeleteHeader<DidHandlerReplyToTheQuery>(out _);

                return Task.CompletedTask;
            };
        }
    }
}