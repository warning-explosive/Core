namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    using System.Data;
    using AutoRegistration.Api.Abstractions;
    using Connection;

    internal interface ITransactionalDatabase : IResolvable
    {
        IAdvancedDbTransaction BeginTransaction(InMemoryDbConnection connection, IsolationLevel isolationLevel);

        void EndTransaction(IAdvancedDbTransaction transaction);
    }
}