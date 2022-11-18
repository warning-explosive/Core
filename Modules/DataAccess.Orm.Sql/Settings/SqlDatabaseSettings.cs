namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Settings
{
    using System.Data;
    using Basics;
    using CrossCuttingConcerns.Settings;

    /// <summary>
    /// SqlDatabaseSettings
    /// </summary>
    public class SqlDatabaseSettings : ISettings
    {
        private const string Format = "{0}={1}";

        /// <summary> .cctor </summary>
        public SqlDatabaseSettings()
        {
            Host = "localhost";
            Port = 5432;
            Database = "AndromedaDatabase";

            Username = "postgres";
            Password = "Password12!";

            IsolationLevel = IsolationLevel.Snapshot;
            ConnectionPoolSize = 100;
        }

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Database
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Isolation level
        ///
        /// The phenomena which are prohibited at various levels are:
        /// [dirty read] -> A transaction reads data written by a concurrent uncommitted transaction
        /// [non-repeatable read] -> A transaction re-reads data it has previously read and finds that data has been modified by another transaction (that committed since the initial read)
        /// [phantom read] -> A transaction re-executes a query returning a set of rows that satisfy a search condition and finds that the set of rows satisfying the condition has changed due to another recently-committed transaction
        /// [serialization anomaly] -> The result of successfully committing a group of transactions is inconsistent with all possible orderings of running those transactions one at a time
        ///
        /// [DB] PostgreSql:
        /// - Chaos -> not supported
        /// - Unspecified -> default, ReadCommitted
        /// - ReadUncommitted -> not supported, behave like ReadCommitted
        /// - Snapshot -> not supported, behave like RepeatableRead
        /// - ReadCommitted -> [non-repeatable read] [phantom read] [serialization anomaly]
        /// - RepeatableRead -> [serialization anomaly]
        /// - Serializable -> none phenomenas
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        /// Connection pool size
        /// </summary>
        public uint ConnectionPoolSize { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this
                .ToPropertyDictionary()
                .ToString(";", pair => Format.Format(pair.Key, pair.Value.ToString() ?? "null"));
        }
    }
}