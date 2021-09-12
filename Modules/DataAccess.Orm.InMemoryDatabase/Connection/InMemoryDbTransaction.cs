namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System.Data;
    using System.Threading;

    internal class InMemoryDbTransaction : IDbTransaction
    {
        private int _commit;

        public InMemoryDbTransaction(
            IDbConnection connection,
            IsolationLevel isolationLevel)
        {
            IsolationLevel = isolationLevel;
            Connection = connection;
            _commit = 0;
        }

        public IDbConnection Connection { get; }

        public IsolationLevel IsolationLevel { get; }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _commit, 1, 1) == 1)
            {
                Commit();
            }
            else
            {
                Rollback();
            }
        }

        public void Commit()
        {
            Interlocked.Exchange(ref _commit, 1);
        }

        public void Rollback()
        {
            Interlocked.Exchange(ref _commit, 0);
        }
    }
}