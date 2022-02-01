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
    internal class MarkOutgoingMessageAsSent : IDomainEventHandler<OutboxMessageHaveBeenSent>
    {
        private readonly IDatabaseContext _databaseContext;

        public MarkOutgoingMessageAsSent(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task Handle(OutboxMessageHaveBeenSent domainEvent, CancellationToken token)
        {
            await _databaseContext
                .Write<OutboxMessage, Guid>()
                .Update(domainEvent.MessageId, message => message.Sent, true, token)
                .ConfigureAwait(false);
        }
    }
}