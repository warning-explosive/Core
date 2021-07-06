namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Singleton)]
    [Dependency(typeof(QueryReplyValidationPipeline))]
    internal class UnitOfWorkPipeline : IMessagePipelineStep, IMessagePipeline
    {
        public UnitOfWorkPipeline(IMessagePipeline decoratee)
        {
            Decoratee = decoratee;
        }

        public IMessagePipeline Decoratee { get; }

        public Task Process(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> producer,
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            return context.UnitOfWork.StartTransaction(
                context,
                ProcessWithinTransaction(producer),
                true,
                token);
        }

        private Func<IAdvancedIntegrationContext, CancellationToken, Task> ProcessWithinTransaction(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> messageHandler)
        {
            return (context, token) => Decoratee.Process(messageHandler, context, token);
        }
    }
}