namespace SpaceEngineers.Core.DataAccess.Orm.Connection
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Primitives;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class DatabaseTransaction : IAdvancedDatabaseTransaction, IDisposable
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;

        private static IDbConnection? _connection;

        [SuppressMessage("Analysis", "CA2213", Justification = "disposed with Interlocked.Exchange")]
        private IDbTransaction? _transaction;

        public DatabaseTransaction(IDatabaseConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public bool HasChanges => throw new NotImplementedException("#131 - track domain entities");

        public IDbTransaction UnderlyingDbTransaction
        {
            get
            {
                _transaction ??= Open(CancellationToken.None).Result;
                return _transaction;
            }
        }

        private IDbConnection Connection
        {
            get
            {
                if (_connection?.State == ConnectionState.Open)
                {
                    return _connection;
                }

                Interlocked.Exchange(ref _connection, default)?.Dispose();

                _connection = _connectionProvider
                    .OpenConnection(CancellationToken.None)
                    .Result;

                return _connection;
            }
        }

        public Task<IDbTransaction> Open(CancellationToken token)
        {
            return Open(_connectionProvider.IsolationLevel, token);
        }

        public Task<IDbTransaction> Open(IsolationLevel isolationLevel, CancellationToken token)
        {
            // TODO: #150 - support manually opened nested transactions
            // TODO: #151 - work with single, app wide connection
            if (_transaction != null)
            {
                throw new InvalidOperationException("Database transaction have already opened");
            }

            _transaction = Connection.BeginTransaction(isolationLevel);
            return Task.FromResult(_transaction);
        }

        public async Task<IAsyncDisposable> Open(bool commit, CancellationToken token)
        {
            await Open(token).ConfigureAwait(false);
            return AsyncDisposable.Create((commit, token), state => Close(state.commit, state.token));
        }

        public async Task<IAsyncDisposable> Open(bool commit, IsolationLevel isolationLevel, CancellationToken token)
        {
            await Open(isolationLevel, token).ConfigureAwait(false);
            return AsyncDisposable.Create((commit, token), state => Close(state.commit, state.token));
        }

        public Task Close(bool commit, CancellationToken token)
        {
            // TODO: #132 - commit changes
            Interlocked.Exchange(ref _transaction, default)?.Dispose();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Close(false, CancellationToken.None).Wait();
        }

        public Task Insert(IAggregate aggregate, CancellationToken token)
        {
            throw new NotImplementedException("#131 - track domain entities");
        }

        public Task Update(IAggregate aggregate, CancellationToken token)
        {
            throw new NotImplementedException("#131 - track domain entities");
        }

        public Task Upsert(IAggregate aggregate, CancellationToken token)
        {
            throw new NotImplementedException("#131 - track domain entities");
        }

        public Task Delete(IAggregate aggregate, CancellationToken token)
        {
            throw new NotImplementedException("#131 - track domain entities");
        }
    }
}