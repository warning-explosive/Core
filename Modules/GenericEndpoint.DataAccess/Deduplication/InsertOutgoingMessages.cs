namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Persisting;
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class InsertOutgoingMessages : IDatabaseStateTransformer<OutboxMessagesAreReadyToBeSent>
    {
        private readonly IJsonSerializer _serializer;
        private readonly IBulkRepository<OutboxMessageDatabaseEntity, Guid> _outboxMessageBulkRepository;

        public InsertOutgoingMessages(
            IJsonSerializer serializer,
            IBulkRepository<OutboxMessageDatabaseEntity, Guid> outboxMessageBulkRepository)
        {
            _serializer = serializer;
            _outboxMessageBulkRepository = outboxMessageBulkRepository;
        }

        public async Task Persist(OutboxMessagesAreReadyToBeSent domainEvent, CancellationToken token)
        {
            var messages = domainEvent
                .OutgoingMessages
                .Select(message => IntegrationMessageDatabaseEntity.Build(message, _serializer))
                .Select(message => new OutboxMessageDatabaseEntity(message.PrimaryKey, message, false))
                .ToList();

            await _outboxMessageBulkRepository
                .Insert(messages, token)
                .ConfigureAwait(false);
        }
    }
}