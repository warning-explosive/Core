namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using CompositionRoot;
    using UnitOfWork;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(ErrorHandlingMiddleware))]
    internal class UnitOfWorkMiddleware : IMessageHandlerMiddleware,
                                          ICollectionResolvable<IMessageHandlerMiddleware>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public UnitOfWorkMiddleware(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public Task Handle(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token)
        {
            return _dependencyContainer
               .Resolve<IIntegrationUnitOfWork>()
               .ExecuteInTransaction(context, next, true, token);
        }
    }
}