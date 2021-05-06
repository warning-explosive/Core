namespace SpaceEngineers.Core.DataAccess.PostgreSql.Internals
{
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Settings;
    using Npgsql;
    using SettingsManager.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class ConnectionFactory : IConnectionFactory
    {
        private readonly ISettingsManager<DatabaseSettings> _databaseSettingsProvider;

        public ConnectionFactory(ISettingsManager<DatabaseSettings> databaseSettingsProvider)
        {
            _databaseSettingsProvider = databaseSettingsProvider;
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
            var databaseSettings = await _databaseSettingsProvider
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