namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using System;
    using System.Collections.Generic;
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
    using CrossCuttingConcerns.Json;
    using DatabaseModel;
    using GenericEndpoint.UnitOfWork;
    using Messaging;
    using Messaging.MessageHeaders;
    using Pipeline;
    using EndpointIdentity = Contract.EndpointIdentity;
    using IntegrationMessage = Messaging.IntegrationMessage;

    [ComponentOverride]
    internal class IntegrationUnitOfWork : AsyncUnitOfWork<IAdvancedIntegrationContext>,
                                           IIntegrationUnitOfWork,
                                           IResolvable<IIntegrationUnitOfWork>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IDatabaseTransaction _transaction;
        private readonly IOutboxDelivery _outboxDelivery;
        private readonly IJsonSerializer _jsonSerializer;

        public IntegrationUnitOfWork(
            EndpointIdentity endpointIdentity,
            IDatabaseTransaction transaction,
            IOutboxDelivery outboxDelivery,
            IOutboxStorage outboxStorage,
            IJsonSerializer jsonSerializer)
        {
            _endpointIdentity = endpointIdentity;
            _transaction = transaction;
            _outboxDelivery = outboxDelivery;
            _jsonSerializer = jsonSerializer;

            OutboxStorage = outboxStorage;
        }

        public IOutboxStorage OutboxStorage { get; }

        private InboxMessage? Inbox { get; set; }

        protected override async Task<EnUnitOfWorkBehavior> Start(IAdvancedIntegrationContext context, CancellationToken token)
        {
            await _transaction.Open(token).ConfigureAwait(false);

            Inbox = await UpsertInbox(context.Message, token).ConfigureAwait(false);

            return Inbox.Handled || Inbox.IsError
                ? EnUnitOfWorkBehavior.SkipProducer
                : EnUnitOfWorkBehavior.Regular;
        }

        protected override async Task Commit(IAdvancedIntegrationContext context, CancellationToken token)
        {
            try
            {
                if (ValidateTransaction(context))
                {
                    await MarkInboxAsHandled(token).ConfigureAwait(false);

                    await PersistOutgoingMessages(OutboxStorage.All(), token).ConfigureAwait(false);

                    await _transaction.Close(true, token).ConfigureAwait(false);

                    await ExecutionExtensions
                       .TryAsync((OutboxStorage.All(), _outboxDelivery), DeliverOutgoingMessages)
                       .Catch<Exception>()
                       .Invoke(token)
                       .ConfigureAwait(false);
                }
                else
                {
                    await Rollback(context, null, token).ConfigureAwait(false);
                }
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
                await _transaction
                   .Close(false, token)
                   .ConfigureAwait(false);
            }
            finally
            {
                OutboxStorage.Clear();
            }
        }

        private async Task<InboxMessage?> UpsertInbox(
            IntegrationMessage integrationMessage,
            CancellationToken token)
        {
            var inbox = await _transaction
               .Read<InboxMessage, Guid>()
               .All()
               .Where(message => message.Message.PrimaryKey == integrationMessage.ReadRequiredHeader<Id>().Value
                              && message.EndpointIdentity.LogicalName == _endpointIdentity.LogicalName
                              && message.EndpointIdentity.InstanceName == _endpointIdentity.InstanceName)
               .SingleOrDefaultAsync(token)
               .ConfigureAwait(false);

            if (inbox != null)
            {
                return inbox;
            }

            inbox = new InboxMessage(Guid.NewGuid(),
                DatabaseModel.IntegrationMessage.Build(integrationMessage, _jsonSerializer),
                _endpointIdentity,
                false,
                false);

            await _transaction
               .Write<InboxMessage, Guid>()
               .Insert(new[] { inbox }, EnInsertBehavior.DoUpdate, token)
               .ConfigureAwait(false);

            return inbox;
        }

        private bool ValidateTransaction(IAdvancedIntegrationContext context)
        {
            var inbox = Inbox.EnsureNotNull("You should start the unit of work before committing it");

            if (inbox.Handled || inbox.IsError)
            {
                return false;
            }

            var isCommand = context.Message.IsCommand();

            if (_transaction.HasChanges && !isCommand)
            {
                throw new InvalidOperationException("Only commands can introduce changes in the database. Message handlers should send commands for that purpose.");
            }

            return true;
        }

        private Task MarkInboxAsHandled(CancellationToken token)
        {
            return _transaction
               .Write<InboxMessage, Guid>()
               .Update(new[] { Inbox.PrimaryKey }, message => message.Handled, true, token);
        }

        private Task PersistOutgoingMessages(
            IReadOnlyCollection<IntegrationMessage> outgoingMessages,
            CancellationToken token)
        {
            var outboxId = Guid.NewGuid();

            var messages = outgoingMessages
               .Select(message => DatabaseModel.IntegrationMessage.Build(message, _jsonSerializer))
               .Select(message => new OutboxMessage(message.PrimaryKey, outboxId, _endpointIdentity, message, false))
               .ToArray();

            return _transaction
               .Write<OutboxMessage, Guid>()
               .Insert(messages, EnInsertBehavior.Default, token);
        }

        private static Task DeliverOutgoingMessages(
            (IReadOnlyCollection<IntegrationMessage>, IOutboxDelivery) state,
            CancellationToken token)
        {
            var (messages, outboxDelivery) = state;
            return outboxDelivery.DeliverMessages(messages, token);
        }
    }
}