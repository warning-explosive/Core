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
    internal class MarkOutgoingMessageAsSent : IDatabaseStateTransformer<OutboxMessageHaveBeenSent>
    {
        private readonly IDatabaseContext _databaseContext;

        public MarkOutgoingMessageAsSent(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task Persist(OutboxMessageHaveBeenSent domainEvent, CancellationToken token)
        {
            await _databaseContext
                .Write<OutboxMessageDatabaseEntity, Guid>()
                .Update(domainEvent.MessageId, message => message.Sent, true, token)
                .ConfigureAwait(false);
        }
    }
}