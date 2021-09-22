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
    internal class MarkInboxMessageAsHandled : IDatabaseStateTransformer<InboxMessageWasHandled>
    {
        private readonly IRepository<InboxMessageDatabaseEntity, Guid> _inboxMessageRepository;

        public MarkInboxMessageAsHandled(IRepository<InboxMessageDatabaseEntity, Guid> inboxMessageRepository)
        {
            _inboxMessageRepository = inboxMessageRepository;
        }

        public Task Persist(InboxMessageWasHandled domainEvent, CancellationToken token)
        {
            return _inboxMessageRepository.Update(domainEvent.MessageId, message => message.Handled, true, token);
        }
    }
}