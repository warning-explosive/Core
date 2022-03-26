namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Persisting;
    using Core.DataAccess.Api.Transaction;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class MarkOutgoingMessagesAsSent : IDomainEventHandler<OutboxMessagesHaveBeenSent>
    {
        private readonly IDatabaseContext _databaseContext;

        public MarkOutgoingMessagesAsSent(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task Handle(OutboxMessagesHaveBeenSent domainEvent, CancellationToken token)
        {
            await _databaseContext
                .BulkWrite<OutboxMessage, Guid>()
                .Update(domainEvent.MessageIds, outbox => outbox.Sent, true, token)
                .ConfigureAwait(false);
        }
    }
}