namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Connection;
    using Contract.Abstractions;

    /// <summary>
    /// OrmModelExtensions
    /// </summary>
    public static class OrmModelExtensions
    {
        /// <summary>
        /// Is the type a view
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type is view or not</returns>
        public static bool IsView(this Type type)
        {
            return typeof(IView).IsAssignableFrom(type);
        }

        /// <summary>
        /// Gets database name from actual settings
        /// </summary>
        /// <param name="connectionFactory">IConnectionFactory</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Database name</returns>
        public static async Task<string> GetDatabaseName(this IConnectionFactory connectionFactory, CancellationToken token)
        {
            var connectionStringBuilder = await connectionFactory
                .GetConnectionString()
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