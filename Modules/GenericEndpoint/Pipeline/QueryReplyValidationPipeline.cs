namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class QueryReplyValidationPipeline : IMessagePipelineStep, IMessagePipeline
    {
        public QueryReplyValidationPipeline(IMessagePipeline decoratee)
        {
            Decoratee = decoratee;
        }

        public IMessagePipeline Decoratee { get; }

        public async Task Process(IntegrationMessage message, IExtendedIntegrationContext context, CancellationToken token)
        {
            await Decoratee.Process(message, context, token).ConfigureAwait(false);

            if (message.IsQuery()
                && !message.HandlerReplied())
            {
                throw new InvalidOperationException("Message handler must reply to the query");
            }
        }
    }
}