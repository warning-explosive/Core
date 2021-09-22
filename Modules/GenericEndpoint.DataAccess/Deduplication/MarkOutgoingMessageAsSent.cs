namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Persisting;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class MarkOutgoingMessageAsSent : IDatabaseStateTransformer<OutboxMessageHaveBeenSent>
    {
        private readonly IRepository<OutboxMessageDatabaseEntity, Guid> _outboxMessageRepository;

        public MarkOutgoingMessageAsSent(IRepository<OutboxMessageDatabaseEntity, Guid> outboxMessageRepository)
        {
            _outboxMessageRepository = outboxMessageRepository;
        }

        public async Task Persist(OutboxMessageHaveBeenSent domainEvent, CancellationToken token)
        {
            await _outboxMessageRepository
                .Update(domainEvent.MessageId, message => message.Sent, true, token)
                .ConfigureAwait(false);
        }
    }
}