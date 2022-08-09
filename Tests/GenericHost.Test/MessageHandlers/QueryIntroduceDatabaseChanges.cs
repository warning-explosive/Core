namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Transaction;
    using DatabaseEntities;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class QueryIntroduceDatabaseChanges : IMessageHandler<Query>,
                                                   IResolvable<IMessageHandler<Query>>
    {
        private readonly IIntegrationContext _context;
        private readonly IDatabaseContext _databaseContext;

        public QueryIntroduceDatabaseChanges(
            IIntegrationContext context,
            IDatabaseContext databaseContext)
        {
            _context = context;
            _databaseContext = databaseContext;
        }

        public async Task Handle(Query message, CancellationToken token)
        {
            await _databaseContext
               .Write<DatabaseEntity, Guid>()
               .Insert(new[] { DatabaseEntity.Generate() }, EnInsertBehavior.Default, token)
               .ConfigureAwait(false);

            await _context
               .Reply(message, new Reply(message.Id), token)
               .ConfigureAwait(false);
        }
    }
}