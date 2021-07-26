namespace SpaceEngineers.Core.DataAccess.Orm.Connection.Abstractions
{
    using System.Data;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IConnectionFactory
    /// </summary>
    public interface IConnectionFactory : IResolvable
    {
        /// <summary>
        /// Opens DB connection
        /// </summary>
        /// <returns>Ongoing operation</returns>
        Task<IDbConnection> OpenConnection();
    }
}