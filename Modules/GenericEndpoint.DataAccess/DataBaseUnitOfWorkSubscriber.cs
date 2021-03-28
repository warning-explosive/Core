namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Core.DataAccess.Contract.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class DataBaseUnitOfWorkSubscriber : IUnitOfWorkSubscriber<IExtendedIntegrationContext>
    {
        private readonly IDatabaseTransactionProvider _databaseTransactionProvider;

        public DataBaseUnitOfWorkSubscriber(IDatabaseTransactionProvider databaseTransactionProvider)
        {
            _databaseTransactionProvider = databaseTransactionProvider;
        }

        public Task OnStart(IExtendedIntegrationContext context, CancellationToken token)
        {
            return _databaseTransactionProvider.OpenTransaction(token);
        }

        public Task OnCommit(IExtendedIntegrationContext context, CancellationToken token)
        {
            return _databaseTransactionProvider.Commit(token);
        }

        public Task OnRollback(IExtendedIntegrationContext context, CancellationToken token)
        {
            return _databaseTransactionProvider.Rollback(token);
        }
    }
}