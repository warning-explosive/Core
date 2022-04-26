namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Persisting;
    using Core.DataAccess.Api.Transaction;
    using CrossCuttingConcerns.Json;
    using DatabaseModel;

    [Component(EnLifestyle.Scoped)]
    internal class InsertOutgoingMessages : IDomainEventHandler<OutboxMessagesAreReadyToBeSent>,
                                            IResolvable<IDomainEventHandler<OutboxMessagesAreReadyToBeSent>>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _serializer;

        public InsertOutgoingMessages(
            IDatabaseContext databaseContext,
            IJsonSerializer serializer)
        {
            _databaseContext = databaseContext;
            _serializer = serializer;
        }

        public async Task Handle(OutboxMessagesAreReadyToBeSent domainEvent, CancellationToken token)
        {
            var messages = domainEvent
                .OutgoingMessages
                .Select(message => IntegrationMessage.Build(message, _serializer))
                .Select(message => new OutboxMessage(
                    message.PrimaryKey,
                    domainEvent.OutboxId,
                    domainEvent.EndpointIdentity,
                    message,
                    false))
                .ToArray();

            await _databaseContext
                .Write<OutboxMessage, Guid>()
                .Insert(messages, EnInsertBehavior.Default, token)
                .ConfigureAwait(false);
        }
    }
}