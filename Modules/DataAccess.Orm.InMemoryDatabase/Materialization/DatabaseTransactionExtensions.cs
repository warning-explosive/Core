namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Materialization
{
    using System;
    using System.Linq;
    using Api.Transaction;
    using Connection;

    internal static class DatabaseTransactionExtensions
    {
        internal static IQueryable All(this IAdvancedDatabaseTransaction transaction, Type type)
        {
            return ((IAdvancedDbTransaction)transaction.UnderlyingDbTransaction).All(type);
        }
    }
}