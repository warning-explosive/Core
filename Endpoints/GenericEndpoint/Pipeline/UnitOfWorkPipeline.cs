namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using CompositionRoot;
    using UnitOfWork;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(QueryReplyValidationPipeline))]
    internal class UnitOfWorkPipeline : IMessagePipelineStep, IMessagePipeline
    {
        private readonly IDependencyContainer _dependencyContainer;

        public UnitOfWorkPipeline(
            IMessagePipeline decoratee,
            IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
            Decoratee = decoratee;
        }

        public IMessagePipeline Decoratee { get; }

        public Task Process(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> producer,
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            return _dependencyContainer
               .Resolve<IIntegrationUnitOfWork>()
               .ExecuteInTransaction(
                    context,
                    Process(producer),
                    true,
                    token);
        }

        private Func<IAdvancedIntegrationContext, CancellationToken, Task> Process(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> messageHandler)
        {
            return (context, token) => Decoratee.Process(messageHandler, context, token);
        }
    }
}