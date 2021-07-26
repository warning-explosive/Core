namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using Npgsql;
    using Orm.Connection.Abstractions;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class ConnectionFactory : IConnectionFactory
    {
        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _databaseSettings;

        public ConnectionFactory(ISettingsProvider<PostgreSqlDatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }

        public async Task<DbConnectionStringBuilder> GetConnectionString()
        {
            var databaseSettings = await _databaseSettings
                .Get()
                .ConfigureAwait(false);

            return new NpgsqlConnectionStringBuilder
            {
                Host = databaseSettings.Host,
                Port = databaseSettings.Port,
                Database = databaseSettings.Database,
                Username = databaseSettings.Username,
                Password = databaseSettings.Password
            };
        }

        public async Task<IDbConnection> OpenConnection()
        {
            var connectionStringBuilder = await GetConnectionString().ConfigureAwait(false);
            var connectionString = connectionStringBuilder.ConnectionString;

            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            return connection;
        }
    }
}