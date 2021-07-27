namespace SpaceEngineers.Core.DataAccess.Orm.Connection.Abstractions
{
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IConnectionFactory
    /// </summary>
    public interface IConnectionFactory : IResolvable
    {
        /// <summary>
        /// Checks does the database exist
        /// </summary>
        /// <returns>Ongoing check operation</returns>
        Task<bool> DoesDatabaseExist();

        /// <summary>
        /// Gets connection string
        /// </summary>
        /// <returns>Ongoing operation</returns>
        Task<DbConnectionStringBuilder> GetConnectionString();

        /// <summary>
        /// Opens DB connection
        /// </summary>
        /// <returns>Ongoing operation</returns>
        Task<IDbConnection> OpenConnection();
    }
}