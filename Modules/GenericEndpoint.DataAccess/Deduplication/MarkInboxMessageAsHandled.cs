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
    internal class MarkInboxMessageAsHandled : IDomainEventHandler<InboxMessageWasHandled>,
                                               IResolvable<IDomainEventHandler<InboxMessageWasHandled>>
    {
        private readonly IDatabaseContext _databaseContext;

        public MarkInboxMessageAsHandled(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task Handle(InboxMessageWasHandled domainEvent, CancellationToken token)
        {
            return _databaseContext
                .Write<InboxMessage, Guid>()
                .Update(new[] { domainEvent.InboxId }, message => message.Handled, true, token);
        }
    }
}