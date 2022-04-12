namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Basics.Enumerations;
    using Basics.Primitives;
    using Contract;
    using Core.DataAccess.Api.Transaction;
    using Deduplication;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.UnitOfWork;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using Pipeline;

    [ComponentOverride]
    internal class IntegrationUnitOfWork : AsyncUnitOfWork<IAdvancedIntegrationContext>,
                                           IIntegrationUnitOfWork,
                                           IResolvable<IIntegrationUnitOfWork>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationTransport _transport;
        private readonly IDatabaseTransaction _transaction;
        private readonly IAggregateFactory<Inbox, InboxAggregateSpecification> _inboxAggregateFactory;

        public IntegrationUnitOfWork(
            EndpointIdentity endpointIdentity,
            IIntegrationTransport transport,
            IDatabaseTransaction transaction,
            IAggregateFactory<Inbox, InboxAggregateSpecification> inboxAggregateFactory,
            IOutboxStorage outboxStorage)
        {
            _endpointIdentity = endpointIdentity;
            _transport = transport;
            _transaction = transaction;
            _inboxAggregateFactory = inboxAggregateFactory;
            OutboxStorage = outboxStorage;
        }

        public IOutboxStorage OutboxStorage { get; }

        private Inbox? Inbox { get; set; }

        protected override async Task<EnUnitOfWorkBehavior> Start(IAdvancedIntegrationContext context, CancellationToken token)
        {
            await _transaction
                .Open(token)
                .ConfigureAwait(false);

            var spec = new InboxAggregateSpecification(context.Message, _endpointIdentity);

            Inbox = await _inboxAggregateFactory.Build(spec, token).ConfigureAwait(false);

            return Inbox.Handled || Inbox.IsError
                ? EnUnitOfWorkBehavior.SkipProducer
                : EnUnitOfWorkBehavior.Regular;
        }

        protected override async Task Commit(IAdvancedIntegrationContext context, CancellationToken token)
        {
            var inbox = Inbox.EnsureNotNull("You should start the unit of work before committing it");

            if (inbox.Handled || inbox.IsError)
            {
                await Rollback(context, default, token).ConfigureAwait(false);
                return;
            }

            var isCommand = context.Message.IsCommand();

            if (_transaction.HasChanges && !isCommand)
            {
                throw new InvalidOperationException("Only commands can introduce changes in the database. Message handlers should send commands for that purpose.");
            }

            inbox.MarkAsHandled();

            await _transaction
                .Track(inbox, token)
                .ConfigureAwait(false);

            var outbox = new Outbox(_endpointIdentity, OutboxStorage.All());

            await _transaction
                .Track(outbox, token)
                .ConfigureAwait(false);

            await _transaction.Close(true, token).ConfigureAwait(false);

            await ExecutionExtensions
               .TryAsync(outbox, DeliverMessages)
               .Catch<Exception>()
               .Invoke(token)
               .ConfigureAwait(false);
        }

        protected override async Task Rollback(IAdvancedIntegrationContext context, Exception? exception, CancellationToken token)
        {
            await _transaction.Close(false, token).ConfigureAwait(false);
        }

        private async Task DeliverMessages(Outbox outbox, CancellationToken token)
        {
            /*
             * TODO: #154 - test without immediate delivery
             */

            if (outbox.OutgoingMessages.Any())
            {
                await using (await _transaction.OpenScope(true, token).ConfigureAwait(false))
                {
                    await outbox
                       .DeliverMessages(_transport, token)
                       .ConfigureAwait(false);
                }
            }
        }
    }
}