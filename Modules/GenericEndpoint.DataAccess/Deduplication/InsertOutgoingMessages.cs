namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Persisting;
    using Core.DataAccess.Api.Transaction;
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class InsertOutgoingMessages : IDatabaseStateTransformer<OutboxMessagesAreReadyToBeSent>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _serializer;

        public InsertOutgoingMessages(IDatabaseContext databaseContext, IJsonSerializer serializer)
        {
            _databaseContext = databaseContext;
            _serializer = serializer;
        }

        public async Task Persist(OutboxMessagesAreReadyToBeSent domainEvent, CancellationToken token)
        {
            var messages = domainEvent
                .OutgoingMessages
                .Select(message => IntegrationMessageDatabaseEntity.Build(message, _serializer))
                .Select(message => new OutboxMessageDatabaseEntity(message.PrimaryKey, message, false))
                .ToList();

            await _databaseContext
                .BulkWrite<OutboxMessageDatabaseEntity, Guid>()
                .Insert(messages, token)
                .ConfigureAwait(false);
        }
    }
}