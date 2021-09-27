namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System;
    using System.Data;
    using Database;

    internal class InMemoryDbConnection : IDbConnection
    {
        private readonly ITransactionalDatabase _inMemoryDatabase;

        public InMemoryDbConnection(string database,
            IsolationLevel isolationLevel,
            ITransactionalDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
            Database = database;
            ConnectionString = $"Database={database};IsolationLevel={isolationLevel}";
            IsolationLevel = isolationLevel;
        }

        public int ConnectionTimeout => throw new NotSupportedException(nameof(ConnectionTimeout));

        public string Database { get; }

        public string ConnectionString { get; set; }

        public ConnectionState State { get; private set; }

        private IsolationLevel IsolationLevel { get; }

        public void Dispose()
        {
            Close();
        }

        public void Open()
        {
            State = ConnectionState.Open;
        }

        public void Close()
        {
            State = ConnectionState.Closed;
        }

        public IDbTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel);
        }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection is not open");
            }

            return _inMemoryDatabase.BeginTransaction(this, isolationLevel);
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException(nameof(ChangeDatabase));
        }

        public IDbCommand CreateCommand()
        {
            throw new NotSupportedException(nameof(CreateCommand));
        }
    }
}