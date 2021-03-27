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

        public Task Process(IExtendedIntegrationContext context, CancellationToken token)
        {
            return context.UnitOfWork.StartTransaction(context, ExecuteWithinTransaction, true, token);
        }

        private Task ExecuteWithinTransaction(IExtendedIntegrationContext context, CancellationToken token)
        {
            return Decoratee.Process(context, token);
        }
    }
}