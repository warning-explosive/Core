namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Enumerations;
    using Basics.Primitives;
    using CrossCuttingConcerns.Logging;
    using Deduplication;
    using Messaging;
    using Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;
    using Pipeline;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Linq;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction;
    using SpaceEngineers.Core.GenericEndpoint.UnitOfWork;
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
        private readonly ILogger _logger;

        public IntegrationUnitOfWork(
            EndpointIdentity endpointIdentity,
            IAdvancedDatabaseTransaction transaction,
            IOutboxDelivery outboxDelivery,
            IOutboxStorage outboxStorage,
            ILogger logger)
        {
            _endpointIdentity = endpointIdentity;
            _transaction = transaction;
            _outboxDelivery = outboxDelivery;
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
                    await PersistInbox(context, _transaction, _endpointIdentity, Inbox, token).ConfigureAwait(false);

                    await PersistOutgoingMessages(_transaction, _endpointIdentity, OutboxStorage.All(), token).ConfigureAwait(false);

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
               .All<InboxMessage>()
               .Where(message => message.Message.PrimaryKey == context.Message.ReadRequiredHeader<Id>().Value
                              && message.EndpointIdentity.LogicalName == endpointIdentity.LogicalName
                              && message.EndpointIdentity.InstanceName == endpointIdentity.InstanceName)
               .CachedExpression("71E74566-4D9F-4767-9CC4-56F04EB76245")
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
            CancellationToken token)
        {
            if (inbox == null)
            {
                inbox = new InboxMessage(Guid.NewGuid(),
                    new Deduplication.IntegrationMessage(context.Message),
                    endpointIdentity,
                    false,
                    true);

                await databaseContext
                   .Insert(new[] { inbox }, EnInsertBehavior.DoNothing)
                   .CachedExpression("98696B21-1D0D-416B-9A39-AFA6AFB16A0A")
                   .Invoke(token)
                   .ConfigureAwait(false);
            }
            else
            {
                await databaseContext
                    .Update<InboxMessage>()
                    .Set(message => message.Handled.Assign(true))
                    .Where(message => message.PrimaryKey == inbox.PrimaryKey)
                    .CachedExpression("45A2D69C-BB68-403C-9A12-037D60959BC2")
                    .Invoke(token)
                    .ConfigureAwait(false);
            }
        }

        private static Task PersistOutgoingMessages(
            IDatabaseContext databaseContext,
            EndpointIdentity endpointIdentity,
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token)
        {
            var outboxId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;

            var outboxMessages = messages
               .Select(message => new Deduplication.IntegrationMessage(message))
               .Select(message => new OutboxMessage(message.PrimaryKey, outboxId, timestamp, endpointIdentity, message, false))
               .ToArray();

            return outboxMessages.Any()
                ? databaseContext
                    .Insert(outboxMessages, EnInsertBehavior.Default)
                    .CachedExpression($"{nameof(PersistOutgoingMessages)}:{outboxMessages.Length}")
                    .Invoke(token)
                : Task.CompletedTask;
        }

        private static Task DeliverOutgoingMessages(
            ILogger logger,
            IOutboxDelivery outboxDelivery,
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token)
        {
            return outboxDelivery
                .DeliverMessages(messages, token)
                .TryAsync()
                .Catch<Exception>(OnCatch(logger))
                .Invoke(token);
        }

        private static Func<Exception, CancellationToken, Task> OnCatch(ILogger logger)
        {
            return (exception, _) =>
            {
                logger.Error(exception, "Outbox delivery error");
                return Task.CompletedTask;
            };
        }
    }
}