namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics.Primitives;
    using Core.DataAccess.Api.Abstractions;
    using Messaging;

    [ComponentOverride]
    internal class DataAccessIntegrationUnitOfWork : AsyncUnitOfWork<IAdvancedIntegrationContext>,
                                                     IIntegrationUnitOfWork
    {
        private readonly IDatabaseTransaction _databaseTransaction;

        public DataAccessIntegrationUnitOfWork(
            IDatabaseTransaction databaseTransaction,
            IOutboxStorage outboxStorage)
        {
            _databaseTransaction = databaseTransaction;
            OutboxStorage = outboxStorage;
        }

        // TODO: #100 - implement transactional outbox pattern & register override
        public IOutboxStorage OutboxStorage { get; }

        protected override Task Start(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task Commit(IAdvancedIntegrationContext context, CancellationToken token)
        {
            var isCommand = context.Message.IsCommand();

            if (_databaseTransaction.HasChanges && !isCommand)
            {
                throw new InvalidOperationException("Only commands can introduce changes in the database. Message handlers should send commands for that purpose.");
            }

            var commit = _databaseTransaction.HasChanges && isCommand;

            await _databaseTransaction
                .Close(commit, token)
                .ConfigureAwait(false);
        }

        protected override async Task Rollback(IAdvancedIntegrationContext context, Exception? exception, CancellationToken token)
        {
            await _databaseTransaction
                .Close(false, token)
                .ConfigureAwait(false);
        }
    }
}