namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Persisting;
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class InsertInboxMessage : IDatabaseStateTransformer<InboxMessageReceived>
    {
        private readonly IRepository<InboxMessageDatabaseEntity, Guid> _inboxMessageRepository;
        private readonly IJsonSerializer _serializer;

        public InsertInboxMessage(
            IRepository<InboxMessageDatabaseEntity, Guid> inboxMessageRepository,
            IJsonSerializer serializer)
        {
            _inboxMessageRepository = inboxMessageRepository;
            _serializer = serializer;
        }

        public Task Persist(InboxMessageReceived domainEvent, CancellationToken token)
        {
            var message = IntegrationMessageDatabaseEntity.Build(domainEvent.Message, _serializer);

            var endpointIdentity = new EndpointIdentityInlinedObject(domainEvent.EndpointIdentity.LogicalName, domainEvent.EndpointIdentity.InstanceName);

            var inbox = new InboxMessageDatabaseEntity(Guid.NewGuid(), message, endpointIdentity, false, false);

            return _inboxMessageRepository.Insert(inbox, token);
        }
    }
}