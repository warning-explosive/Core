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

    /// <summary>
    /// UnitOfWorkMiddleware
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    [After(typeof(ErrorHandlingMiddleware))]
    public class UnitOfWorkMiddleware : IMessageHandlerMiddleware,
                                        ICollectionResolvable<IMessageHandlerMiddleware>
    {
        private readonly IDependencyContainer _dependencyContainer;

        /// <summary> .cctor </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        public UnitOfWorkMiddleware(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        /// <inheritdoc />
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