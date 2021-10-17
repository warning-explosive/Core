﻿namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
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
    internal class MarkInboxMessageAsHandled : IDatabaseStateTransformer<InboxMessageWasHandled>
    {
        private readonly IDatabaseContext _databaseContext;

        public MarkInboxMessageAsHandled(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task Persist(InboxMessageWasHandled domainEvent, CancellationToken token)
        {
            return _databaseContext
                .Write<InboxMessage, Guid>()
                .Update(domainEvent.InboxId, message => message.Handled, true, token);
        }
    }
}