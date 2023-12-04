namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Enumerations;
    using Basics.Primitives;
    using CrossCuttingConcerns.Logging;
    using Deduplication;
    using GenericEndpoint.Pipeline;
    using Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;
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
        private readonly ITransactionalOutbox _outbox;
        private readonly ILogger _logger;

        public IntegrationUnitOfWork(
            EndpointIdentity endpointIdentity,
            IAdvancedDatabaseTransaction transaction,
            ITransactionalOutbox outbox,
            ILogger logger)
        {
            _endpointIdentity = endpointIdentity;
            _transaction = transaction;
            _outbox = outbox;
            _logger = logger;
        }

        private InboxMessage? Inbox { get; set; }

        protected override async Task<EnUnitOfWorkBehavior> Start(
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            await _transaction.Open(token).ConfigureAwait(false);

            Inbox = await ReadInbox(context, _transaction, _endpointIdentity, token).ConfigureAwait(false);

            return Inbox == null || Inbox.State == EnInboxMessageState.Processing
                ? EnUnitOfWorkBehavior.Regular
                : EnUnitOfWorkBehavior.SkipProducer;
        }

        protected override async Task Commit(
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            await (Inbox == null
                ? PersistInbox(context, _transaction, _endpointIdentity, EnInboxMessageState.Handled, token)
                : MarkInboxAsHandled(_transaction, Inbox.PrimaryKey, token)).ConfigureAwait(false);

            await PersistOutgoingMessages(_transaction, _endpointIdentity, _outbox.All(), token).ConfigureAwait(false);

            await _transaction.Close(true, token).ConfigureAwait(false);

            await using (await _transaction.OpenScope(true, token).ConfigureAwait(false))
            {
                await DeliverOutgoingMessages(_outbox, _logger, token).ConfigureAwait(false);
            }
        }

        protected override async Task Rollback(
            IAdvancedIntegrationContext context,
            Exception? exception,
            CancellationToken token)
        {
            await _transaction.Close(false, token).ConfigureAwait(false);
        }

        private static Task<InboxMessage?> ReadInbox(
            IAdvancedIntegrationContext context,
            IDatabaseContext databaseContext,
            EndpointIdentity endpointIdentity,
            CancellationToken token)
        {
            return databaseContext
                .All<InboxMessage>()
                .Where(inbox => inbox.Message.PrimaryKey == context.Message.ReadRequiredHeader<Id>().Value
                                && inbox.EndpointLogicalName == endpointIdentity.LogicalName
                                && inbox.EndpointInstanceName == endpointIdentity.InstanceName)
                .CachedExpression("71E74566-4D9F-4767-9CC4-56F04EB76245")
                .SingleOrDefaultAsync(token);
        }

        private static Task PersistInbox(
            IAdvancedIntegrationContext context,
            IDatabaseContext databaseContext,
            EndpointIdentity endpointIdentity,
            EnInboxMessageState state,
            CancellationToken token)
        {
            var inbox = new InboxMessage(
                Guid.NewGuid(),
                new Deduplication.IntegrationMessage(context.Message),
                endpointIdentity.LogicalName,
                endpointIdentity.InstanceName,
                state);

            return databaseContext
                .Insert(new[] { inbox }, EnInsertBehavior.DoNothing)
                .CachedExpression($"{nameof(PersistInbox)}:{inbox.Message.Headers.Count}:585AA6A8-17F4-44EE-9747-55D504E33299")
                .Invoke(token);
        }

        private static Task MarkInboxAsHandled(
            IDatabaseContext databaseContext,
            Guid primaryKey,
            CancellationToken token)
        {
            return databaseContext
                .Update<InboxMessage>()
                .Set(inbox => inbox.State.Assign(EnInboxMessageState.Handled))
                .Where(inbox => inbox.PrimaryKey == primaryKey)
                .CachedExpression("45A2D69C-BB68-403C-9A12-037D60959BC2")
                .Invoke(token);
        }

        private static async Task PersistOutgoingMessages(
            IDatabaseContext databaseContext,
            EndpointIdentity endpointIdentity,
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token)
        {
            var outboxId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;

            var outboxMessages = messages
               .Select(message => new Deduplication.IntegrationMessage(message))
               .Select(message => new OutboxMessage(message.PrimaryKey, outboxId, timestamp, endpointIdentity.LogicalName, endpointIdentity.InstanceName, message, false))
               .ToArray();

            if (!outboxMessages.Any())
            {
                return;
            }

            foreach (var outboxMessage in outboxMessages)
            {
                await databaseContext
                    .Insert(new[] { outboxMessage }, EnInsertBehavior.Default)
                    .CachedExpression($"{nameof(PersistOutgoingMessages)}:{outboxMessage.Message.Headers.Count}:6C9A240C-A104-4712-87FA-8631A273C57D")
                    .Invoke(token)
                    .ConfigureAwait(false);
            }
        }

        private static Task DeliverOutgoingMessages(
            ITransactionalOutbox outbox,
            ILogger logger,
            CancellationToken token)
        {
            return outbox
                .DeliverMessages(token)
                .TryAsync()
                .Catch<Exception>(OnCatch(logger))
                .Invoke(token);

            static Func<Exception, CancellationToken, Task> OnCatch(ILogger logger)
            {
                return (exception, _) =>
                {
                    logger.Error(exception, "Outbox delivery error");
                    return Task.CompletedTask;
                };
            }
        }
    }
}