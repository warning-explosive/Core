namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
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

        public Task Process(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return context.UnitOfWork.StartTransaction(context,
                ProcessWithinTransaction,
                true,
                token);
        }

        private Task ProcessWithinTransaction(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return Decoratee.Process(context, token);
        }
    }
}