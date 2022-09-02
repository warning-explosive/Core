namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System.Data;
    using Api.Transaction;

    /// <summary>
    /// DatabaseConnection
    /// </summary>
    public class DatabaseConnection : IDatabaseConnection
    {
        /// <summary> .cctor </summary>
        /// <param name="connection">IDbConnection</param>
        public DatabaseConnection(IDbConnection connection)
        {
            UnderlyingDbConnection = connection;
        }

        /// <inheritdoc />
        public IDbConnection UnderlyingDbConnection { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            UnderlyingDbConnection.Dispose();
        }
    }
}