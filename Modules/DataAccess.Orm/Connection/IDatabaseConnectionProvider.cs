namespace SpaceEngineers.Core.DataAccess.Orm.Connection
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IDatabaseConnectionProvider
    /// </summary>
    public interface IDatabaseConnectionProvider : IResolvable
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
        /// Checks does the database exist
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing check operation</returns>
        Task<bool> DoesDatabaseExist(CancellationToken token);

        /// <summary>
        /// Opens DB connection
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IDbConnection> OpenConnection(CancellationToken token);
    }
}