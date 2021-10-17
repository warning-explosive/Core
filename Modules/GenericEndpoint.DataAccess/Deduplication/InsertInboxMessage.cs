namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Persisting;
    using Core.DataAccess.Api.Transaction;
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class InsertInboxMessage : IDatabaseStateTransformer<InboxMessageReceived>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _serializer;

        public InsertInboxMessage(IDatabaseContext databaseContext, IJsonSerializer serializer)
        {
            _databaseContext = databaseContext;
            _serializer = serializer;
        }

        public Task Persist(InboxMessageReceived domainEvent, CancellationToken token)
        {
            var message = IntegrationMessage.Build(domainEvent.Message, _serializer);

            var inbox = new InboxMessage(domainEvent.InboxId, message, domainEvent.EndpointIdentity, false, false);

            return _databaseContext
                .Write<InboxMessage, Guid>()
                .Insert(inbox, token);
        }
    }
}