namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Core.DataAccess.Contract.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class DataBaseUnitOfWorkSubscriber : IUnitOfWorkSubscriber<IAdvancedIntegrationContext>
    {
        private readonly IDatabaseTransaction _databaseTransaction;

        public DataBaseUnitOfWorkSubscriber(IDatabaseTransaction databaseTransaction)
        {
            _databaseTransaction = databaseTransaction;
        }

        public Task OnStart(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task OnCommit(IAdvancedIntegrationContext context, CancellationToken token)
        {
            var isCommand = context.Message.IsCommand();

            if (_databaseTransaction.HasChanges
                && !isCommand)
            {
                throw new InvalidOperationException("Only commands can introduce changes in the database. Message handlers should send commands for that purpose.");
            }

            await _databaseTransaction
                .Close(isCommand)
                .ConfigureAwait(false);
        }

        public async Task OnRollback(IAdvancedIntegrationContext context, CancellationToken token)
        {
            await _databaseTransaction
                .Close(false)
                .ConfigureAwait(false);
        }
    }
}