namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Persisting;
    using Core.DataAccess.Api.Transaction;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class MarkInboxMessageAsRefused : IDomainEventHandler<InboxMessageWasMovedToErrorQueue>,
                                               IResolvable<IDomainEventHandler<InboxMessageWasMovedToErrorQueue>>
    {
        private readonly IDatabaseContext _databaseContext;

        public MarkInboxMessageAsRefused(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task Handle(InboxMessageWasMovedToErrorQueue domainEvent, CancellationToken token)
        {
            return _databaseContext
                .Write<InboxMessage, Guid>()
                .Update(new[] { domainEvent.InboxId }, message => message.IsError, true, token);
        }
    }
}