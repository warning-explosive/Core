namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericDomain.Api.Abstractions;
    using Npgsql;
    using Settings;
    using Sql.Connection;

    [Component(EnLifestyle.Scoped)]
    internal class DatabaseTransaction : IDatabaseTransaction, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _databaseSettings;

        private NpgsqlConnection? _connection;
        private NpgsqlTransaction? _transaction;

        public DatabaseTransaction(
            IConnectionFactory connectionFactory,
            ISettingsProvider<PostgreSqlDatabaseSettings> databaseSettings)
        {
            _connectionFactory = connectionFactory;
            _databaseSettings = databaseSettings;
        }

        public bool HasChanges => throw new NotImplementedException("#131 - track domain entities");

        public Task Track(IAggregate aggregate, CancellationToken token)
        {
            throw new NotImplementedException("#131 - track domain entities");
        }

        public async Task<IDbTransaction> Open(CancellationToken token)
        {
            // TODO: #150 - support manually opened nested transactions
            // TODO: #151 - work with single, app wide connection
            if (_connection?.State == ConnectionState.Open
                && _transaction != null)
            {
                return _transaction;
            }

            var postgreSqlSettings = await _databaseSettings
                .Get(token)
                .ConfigureAwait(false);

            if (_connection != null)
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
            }

            _connection = (NpgsqlConnection)await _connectionFactory
                .OpenConnection(token)
                .ConfigureAwait(false);

            if (_transaction != null)
            {
                await _transaction.DisposeAsync().ConfigureAwait(false);
            }

            _transaction = await _connection
                .BeginTransactionAsync(postgreSqlSettings.IsolationLevel, token)
                .ConfigureAwait(false);

            return _transaction;
        }

        public async Task Close(bool commit, CancellationToken token)
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync().ConfigureAwait(false);
            }

            if (_connection != null)
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            Close(false, CancellationToken.None).Wait();
        }
    }
}