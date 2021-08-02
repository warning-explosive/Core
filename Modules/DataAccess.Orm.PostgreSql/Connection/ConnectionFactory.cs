namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.Api.Abstractions;
    using Npgsql;
    using Orm.Connection;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class ConnectionFactory : IConnectionFactory
    {
        private const string SqlState = nameof(SqlState);
        private const string DatabaseDoesNotExistCode = "3D000";

        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _databaseSettings;

        public ConnectionFactory(ISettingsProvider<PostgreSqlDatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }

        public Task<bool> DoesDatabaseExist()
        {
            return ExecutionExtensions
                .TryAsync(DoesDatabaseExistUnsafe)
                .Catch<PostgresException>()
                .Invoke(DatabaseDoesNotExist);
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

        public async Task<bool> DoesDatabaseExistUnsafe()
        {
            using (await OpenConnection().ConfigureAwait(false))
            {
                return true;
            }
        }

        private static Task<bool> DatabaseDoesNotExist(Exception ex)
        {
            return ex.Data.Contains(SqlState)
                   && ex.Data[SqlState] is string sqlStateCode
                   && sqlStateCode.Equals(DatabaseDoesNotExistCode, StringComparison.OrdinalIgnoreCase)
                ? Task.FromResult(false)
                : Task.FromResult(true);
        }
    }
}