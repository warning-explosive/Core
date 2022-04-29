namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System.Data;
    using Api.Transaction;

    internal class DatabaseConnection : IDatabaseConnection
    {
        public DatabaseConnection(IDbConnection connection)
        {
            UnderlyingDbConnection = connection;
        }

        public IDbConnection UnderlyingDbConnection { get; }

        public void Dispose()
        {
            UnderlyingDbConnection.Dispose();
        }
    }
}