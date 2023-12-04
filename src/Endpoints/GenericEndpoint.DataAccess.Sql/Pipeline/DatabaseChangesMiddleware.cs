namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using CompositionRoot;
    using Core.DataAccess.Orm.Sql.Transaction;
    using GenericEndpoint.Pipeline;
    using Messaging;

    /// <summary>
    /// DatabaseChangesMiddleware
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    [After(typeof(UnitOfWorkMiddleware))]
    public class DatabaseChangesMiddleware : IMessageHandlerMiddleware,
                                             ICollectionResolvable<IMessageHandlerMiddleware>
    {
        private readonly IDependencyContainer _dependencyContainer;

        /// <summary> .cctor </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        public DatabaseChangesMiddleware(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        /// <inheritdoc />
        public async Task Handle(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token)
        {
            await next
                .Invoke(context, token)
                .ConfigureAwait(false);

            if (_dependencyContainer.Resolve<IAdvancedDatabaseTransaction>().HasChanges
                && !context.Message.IsCommand())
            {
                throw new InvalidOperationException("Only commands can introduce changes in the database. Message handlers should send commands for that purpose.");
            }
        }
    }
}