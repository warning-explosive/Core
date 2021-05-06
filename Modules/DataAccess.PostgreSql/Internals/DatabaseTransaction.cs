namespace SpaceEngineers.Core.DataAccess.PostgreSql.Internals
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;
    using Npgsql;
    using Settings;
    using SettingsManager.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class DatabaseTransaction : IDatabaseTransaction, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ISettingsManager<PostgreSqlSettings> _postgreSqlSettingsProvider;

        private NpgsqlConnection? _connection;
        private NpgsqlTransaction? _transaction;

        public DatabaseTransaction(
            IConnectionFactory connectionFactory,
            ISettingsManager<PostgreSqlSettings> postgreSqlSettingsProvider)
        {
            _connectionFactory = connectionFactory;
            _postgreSqlSettingsProvider = postgreSqlSettingsProvider;
        }

        public bool HasChanges => throw new NotImplementedException();

        public async Task<IDbTransaction> Open(CancellationToken token)
        {
            if (_connection?.State == ConnectionState.Open
                && _transaction != null)
            {
                return _transaction;
            }

            var postgreSqlSettings = await _postgreSqlSettingsProvider
                .Get()
                .ConfigureAwait(false);

            if (_connection != null)
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
            }

            _connection = await _connectionFactory
                .OpenConnection()
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