namespace SpaceEngineers.Core.DataAccess.Orm.Connection
{
    using System.Data;

    /// <summary>
    /// DatabaseConnection
    /// </summary>
    public class DatabaseConnection : IDatabaseConnection
    {
        /// <summary> .cctor </summary>
        /// <param name="connection">IDbConnection</param>
        public DatabaseConnection(IDbConnection connection)
        {
            DbConnection = connection;
        }

        /// <inheritdoc />
        public IDbConnection DbConnection { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            DbConnection.Dispose();
        }
    }
}