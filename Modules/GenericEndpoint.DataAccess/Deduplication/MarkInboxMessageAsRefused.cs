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
    internal class MarkInboxMessageAsRefused : IDatabaseStateTransformer<InboxMessageWasMovedToErrorQueue>
    {
        private readonly IRepository<InboxMessageDatabaseEntity, Guid> _inboxMessageRepository;

        public MarkInboxMessageAsRefused(IRepository<InboxMessageDatabaseEntity, Guid> inboxMessageRepository)
        {
            _inboxMessageRepository = inboxMessageRepository;
        }

        public Task Persist(InboxMessageWasMovedToErrorQueue domainEvent, CancellationToken token)
        {
            return _inboxMessageRepository.Update(domainEvent.MessageId, message => message.IsError, true, token);
        }
    }
}