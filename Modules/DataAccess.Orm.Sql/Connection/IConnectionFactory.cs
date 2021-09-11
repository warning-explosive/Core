namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Connection
{
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IConnectionFactory
    /// </summary>
    public interface IConnectionFactory : IResolvable
    {
        /// <summary>
        /// Checks does the database exist
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing check operation</returns>
        Task<bool> DoesDatabaseExist(CancellationToken token);

        /// <summary>
        /// Gets connection string
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<DbConnectionStringBuilder> GetConnectionString(CancellationToken token);

        /// <summary>
        /// Opens DB connection
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IDbConnection> OpenConnection(CancellationToken token);
    }
}