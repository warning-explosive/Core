namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericDomain.Abstractions;
    using Npgsql;
    using Orm.Connection;
    using Settings;

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

        public bool HasChanges => throw new NotImplementedException();

        public async Task<IDbTransaction> Open(CancellationToken token)
        {
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

        public void Track<TAggregate>(TAggregate aggregate)
            where TAggregate : class, IAggregate
        {
            throw new NotImplementedException();
        }
    }
}