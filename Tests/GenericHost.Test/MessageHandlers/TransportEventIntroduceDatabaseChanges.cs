namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
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
    internal class TransportEventIntroduceDatabaseChanges : IMessageHandler<TransportEvent>,
                                                            IResolvable<IMessageHandler<TransportEvent>>
    {
        private readonly IDatabaseContext _databaseContext;

        public TransportEventIntroduceDatabaseChanges(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task Handle(TransportEvent message, CancellationToken token)
        {
            return _databaseContext.Insert<DatabaseEntity>(new[] { DatabaseEntity.Generate() }, EnInsertBehavior.Default, token);
        }
    }
}