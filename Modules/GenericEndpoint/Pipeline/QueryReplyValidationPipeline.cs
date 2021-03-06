namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Messaging;

    [Component(EnLifestyle.Singleton)]
    internal class QueryReplyValidationPipeline : IMessagePipelineStep, IMessagePipeline
    {
        public QueryReplyValidationPipeline(IMessagePipeline decoratee)
        {
            Decoratee = decoratee;
        }

        public IMessagePipeline Decoratee { get; }

        public async Task Process(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> producer,
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            await Decoratee
                .Process(producer, context, token)
                .ConfigureAwait(false);

            if (context.Message.IsQuery()
                && !context.Message.DidHandlerReply())
            {
                throw new InvalidOperationException("Message handler should reply to the query");
            }
        }
    }
}