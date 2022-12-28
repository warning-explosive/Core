namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Basics.Enumerations;
    using Basics.Primitives;
    using Core.DataAccess.Api.Persisting;
    using Core.DataAccess.Api.Reading;
    using Core.DataAccess.Api.Transaction;
    using CrossCuttingConcerns.Extensions;
    using CrossCuttingConcerns.Json;
    using Deduplication;
    using GenericEndpoint.UnitOfWork;
    using Messaging.Extensions;
    using Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;
    using Pipeline;
    using EndpointIdentity = Contract.EndpointIdentity;
    using IntegrationMessage = Messaging.IntegrationMessage;

    [ComponentOverride]
    internal class IntegrationUnitOfWork : AsyncUnitOfWork<IAdvancedIntegrationContext>,
                                           IIntegrationUnitOfWork,
                                           IResolvable<IIntegrationUnitOfWork>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IAdvancedDatabaseTransaction _transaction;
        private readonly IOutboxDelivery _outboxDelivery;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;

        public IntegrationUnitOfWork(
            EndpointIdentity endpointIdentity,
            IAdvancedDatabaseTransaction transaction,
            IOutboxDelivery outboxDelivery,
            IOutboxStorage outboxStorage,
            IJsonSerializer jsonSerializer,
            ILogger logger)
        {
            _endpointIdentity = endpointIdentity;
            _transaction = transaction;
            _outboxDelivery = outboxDelivery;
            _jsonSerializer = jsonSerializer;
            _logger = logger;

            OutboxStorage = outboxStorage;
        }

        public IOutboxStorage OutboxStorage { get; }

        private InboxMessage? Inbox { get; set; }

        protected override async Task<EnUnitOfWorkBehavior> Start(
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            await _transaction.Open(token).ConfigureAwait(false);

            Inbox = await ReadInbox(context, _transaction, _endpointIdentity, token).ConfigureAwait(false);

            return Inbox != null && (Inbox.Handled || Inbox.IsError)
                ? EnUnitOfWorkBehavior.SkipProducer
                : EnUnitOfWorkBehavior.Regular;
        }

        protected override async Task Commit(
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            try
            {
                if (IsTransactionValid(context, _transaction, out var exception))
                {
                    await PersistInbox(context, _transaction, _endpointIdentity, Inbox, _jsonSerializer, token).ConfigureAwait(false);

                    await PersistOutgoingMessages(_transaction, _endpointIdentity, OutboxStorage.All(), _jsonSerializer, token).ConfigureAwait(false);

                    await _transaction.Close(true, token).ConfigureAwait(false);
                }
                else
                {
                    await _transaction.Close(false, token).ConfigureAwait(false);

                    throw exception.Rethrow();
                }

                await DeliverOutgoingMessages(_logger, _outboxDelivery, OutboxStorage.All(), token).ConfigureAwait(false);
            }
            finally
            {
                OutboxStorage.Clear();
            }
        }

        protected override async Task Rollback(
            IAdvancedIntegrationContext context,
            Exception? exception,
            CancellationToken token)
        {
            try
            {
                await _transaction.Close(false, token).ConfigureAwait(false);
            }
            finally
            {
                OutboxStorage.Clear();
            }
        }

        private static Task<InboxMessage?> ReadInbox(
            IAdvancedIntegrationContext context,
            IDatabaseContext databaseContext,
            EndpointIdentity endpointIdentity,
            CancellationToken token)
        {
            return databaseContext
               .Read<InboxMessage>()
               .All()
               .Where(message => message.Message.PrimaryKey == context.Message.ReadRequiredHeader<Id>().Value
                              && message.EndpointIdentity.LogicalName == endpointIdentity.LogicalName
                              && message.EndpointIdentity.InstanceName == endpointIdentity.InstanceName)
               .SingleOrDefaultAsync(token);
        }

        private static bool IsTransactionValid(
            IAdvancedIntegrationContext context,
            IAdvancedDatabaseTransaction transaction,
            [NotNullWhen(false)] out Exception? exception)
        {
            if (transaction.HasChanges && !context.Message.IsCommand())
            {
                exception = new InvalidOperationException("Only commands can introduce changes in the database. Message handlers should send commands for that purpose.");
                return false;
            }

            exception = null;
            return true;
        }

        private static async Task PersistInbox(
            IAdvancedIntegrationContext context,
            IDatabaseContext databaseContext,
            EndpointIdentity endpointIdentity,
            InboxMessage? inbox,
            IJsonSerializer jsonSerializer,
            CancellationToken token)
        {
            if (inbox == null)
            {
                inbox = new InboxMessage(Guid.NewGuid(),
                    Deduplication.IntegrationMessage.Build(context.Message, jsonSerializer),
                    endpointIdentity,
                    false,
                    true);

                await databaseContext
                   .Insert<InboxMessage>(new[] { inbox }, EnInsertBehavior.DoUpdate, token)
                   .ConfigureAwait(false);
            }
            else
            {
                await databaseContext
                    .Update<InboxMessage, bool>(message => message.Handled, _ => true, message => message.PrimaryKey == inbox.PrimaryKey, token)
                    .ConfigureAwait(false);
            }
        }

        private static Task PersistOutgoingMessages(
            IDatabaseContext databaseContext,
            EndpointIdentity endpointIdentity,
            IReadOnlyCollection<IntegrationMessage> messages,
            IJsonSerializer jsonSerializer,
            CancellationToken token)
        {
            var outboxId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;

            var outboxMessages = messages
               .Select(message => Deduplication.IntegrationMessage.Build(message, jsonSerializer))
               .Select(message => new OutboxMessage(message.PrimaryKey, outboxId, timestamp, endpointIdentity, message, false))
               .ToArray();

            return databaseContext.Insert<OutboxMessage>(outboxMessages, EnInsertBehavior.Default, token);
        }

        private static Task DeliverOutgoingMessages(
            ILogger logger,
            IOutboxDelivery outboxDelivery,
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token)
        {
            return ExecutionExtensions
               .TryAsync((outboxDelivery, messages), DeliverOutgoingMessagesUnsafe)
               .Catch<Exception>((exception, _) =>
               {
                   logger.Error(exception, "Outbox delivery error");
                   return Task.CompletedTask;
               })
               .Invoke(token);

            static Task DeliverOutgoingMessagesUnsafe(
                (IOutboxDelivery, IReadOnlyCollection<IntegrationMessage>) state,
                CancellationToken token)
            {
                var (outboxDelivery, messages) = state;
                return outboxDelivery.DeliverMessages(messages, token);
            }
        }
    }
}