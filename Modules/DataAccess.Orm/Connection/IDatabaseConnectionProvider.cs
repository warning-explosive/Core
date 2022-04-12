namespace SpaceEngineers.Core.DataAccess.Orm.Connection
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IDatabaseConnectionProvider
    /// </summary>
    public interface IDatabaseConnectionProvider
    {
        /// <summary>
        /// Host
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Database
        /// </summary>
        string Database { get; }

        /// <summary>
        /// Isolation level
        /// </summary>
        IsolationLevel IsolationLevel { get; }

        /// <summary>
        /// Checks the database existence
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<bool> DoesDatabaseExist(CancellationToken token);

        /// <summary>
        /// Opens DB connection
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IDbConnection> OpenConnection(CancellationToken token);
    }
}