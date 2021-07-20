namespace SpaceEngineers.Core.DataAccess.PostgreSql.Internals
{
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using Npgsql;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class ConnectionFactory : IConnectionFactory
    {
        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _databaseSettings;

        public ConnectionFactory(ISettingsProvider<PostgreSqlDatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }

        public async Task<NpgsqlConnection> OpenConnection()
        {
            var connectionString = await GetConnectionString().ConfigureAwait(false);
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        private async Task<string> GetConnectionString()
        {
            var databaseSettings = await _databaseSettings
                .Get()
                .ConfigureAwait(false);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseSettings.Host,
                Port = databaseSettings.Port,
                Database = databaseSettings.Database,
                Username = databaseSettings.Username,
                Password = databaseSettings.Password
            };

            return connectionStringBuilder.ConnectionString;
        }
    }
}