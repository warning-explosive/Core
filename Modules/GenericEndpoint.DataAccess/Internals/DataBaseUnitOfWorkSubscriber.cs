namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Core.DataAccess.Contract.Abstractions;
    using GenericEndpoint.Abstractions;
    using Messaging;

    [Component(EnLifestyle.Scoped)]
    internal class DataBaseUnitOfWorkSubscriber : IUnitOfWorkSubscriber<IAdvancedIntegrationContext>,
                                                  ICollectionResolvable<IUnitOfWorkSubscriber<IAdvancedIntegrationContext>>
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
                .Close(isCommand, token)
                .ConfigureAwait(false);
        }

        public async Task OnRollback(IAdvancedIntegrationContext context, CancellationToken token)
        {
            await _databaseTransaction
                .Close(false, token)
                .ConfigureAwait(false);
        }
    }
}