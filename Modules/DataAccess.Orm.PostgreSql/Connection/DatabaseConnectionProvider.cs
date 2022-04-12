namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.Settings;
    using Npgsql;
    using Orm.Connection;
    using Sql.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseConnectionProvider : IDatabaseConnectionProvider,
                                                IResolvable<IDatabaseConnectionProvider>
    {
        private const string SqlState = nameof(SqlState);
        private const string DatabaseDoesNotExistCode = "3D000";

        private readonly ISettingsProvider<SqlDatabaseSettings> _settingsProvider;

        public DatabaseConnectionProvider(ISettingsProvider<SqlDatabaseSettings> settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public string Host => _settingsProvider.Get(CancellationToken.None).Result.Host;

        public string Database => _settingsProvider.Get(CancellationToken.None).Result.Database;

        public IsolationLevel IsolationLevel => _settingsProvider.Get(CancellationToken.None).Result.IsolationLevel;

        public Task<bool> DoesDatabaseExist(CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(DoesDatabaseExistUnsafe)
                .Catch<PostgresException>()
                .Invoke(DatabaseDoesNotExist, token);
        }

        public async Task<IDbConnection> OpenConnection(CancellationToken token)
        {
            /*
             * TODO: #151 - add connection count limits
             */

            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = settings.Host,
                Port = settings.Port,
                Database = settings.Database,
                Username = settings.Username,
                Password = settings.Password
            };

            var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
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
    }
}