namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Connection;

    /// <summary>
    /// SqlDatabaseModelExtensions
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Gets database name
        /// </summary>
        /// <param name="connectionFactory">IConnectionFactory</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Database name</returns>
        public static async Task<string> GetDatabaseName(
            this IConnectionFactory connectionFactory,
            CancellationToken token)
        {
            var connectionStringBuilder = await connectionFactory
                .GetConnectionString(token)
                .ConfigureAwait(false);

            if (connectionStringBuilder.TryGetValue("Database", out object value)
                && value is string database)
            {
                return database;
            }

            throw new InvalidOperationException("Cannot find database name in the settings");
        }
    }
}