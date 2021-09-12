namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.Api.Abstractions;
    using Npgsql;
    using Orm.Connection;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseConnectionProvider : IDatabaseConnectionProvider
    {
        private const string SqlState = nameof(SqlState);
        private const string DatabaseDoesNotExistCode = "3D000";

        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _settingsProvider;

        public DatabaseConnectionProvider(ISettingsProvider<PostgreSqlDatabaseSettings> settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public string Database => Settings(CancellationToken.None).Result.Database;

        public IsolationLevel IsolationLevel => Settings(CancellationToken.None).Result.IsolationLevel;

        public Task<bool> DoesDatabaseExist(CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(DoesDatabaseExistUnsafe)
                .Catch<PostgresException>()
                .Invoke(DatabaseDoesNotExist, token);
        }

        public async Task<IDbConnection> OpenConnection(CancellationToken token)
        {
            var connectionStringBuilder = await GetConnectionString(token).ConfigureAwait(false);
            var connectionString = connectionStringBuilder.ConnectionString;

            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(token).ConfigureAwait(false);

            return connection;
        }

        private async Task<bool> DoesDatabaseExistUnsafe(CancellationToken token)
        {
            using (await OpenConnection(token).ConfigureAwait(false))
            {
                return true;
            }
        }

        private static Task<bool> DatabaseDoesNotExist(Exception ex, CancellationToken token)
        {
            return ex.Data.Contains(SqlState)
                   && ex.Data[SqlState] is string sqlStateCode
                   && sqlStateCode.Equals(DatabaseDoesNotExistCode, StringComparison.OrdinalIgnoreCase)
                ? Task.FromResult(false)
                : Task.FromResult(true);
        }

        private Task<PostgreSqlDatabaseSettings> Settings(CancellationToken token)
        {
            return _settingsProvider.Get(token);
        }

        private async Task<DbConnectionStringBuilder> GetConnectionString(CancellationToken token)
        {
            var databaseSettings = await Settings(token).ConfigureAwait(false);

            return new NpgsqlConnectionStringBuilder
            {
                Host = databaseSettings.Host,
                Port = databaseSettings.Port,
                Database = databaseSettings.Database,
                Username = databaseSettings.Username,
                Password = databaseSettings.Password
            };
        }
    }
}