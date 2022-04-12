namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Settings
{
    using System.Data;
    using Basics;
    using CrossCuttingConcerns.Settings;

    /// <summary>
    /// SqlDatabaseSettings
    /// </summary>
    public class SqlDatabaseSettings : IYamlSettings
    {
        private const string Format = "{0}={1}";

        /// <summary> .cctor </summary>
        public SqlDatabaseSettings()
        {
            Host = "localhost";
            Port = 5432;
            Database = "SpaceEngineersDatabase";

            /* TODO: #130 - use secrets or credentials vault */
            Username = "SpaceEngineer";
            Password = "Voyager1";

            IsolationLevel = IsolationLevel.Snapshot;
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
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this
                .ToPropertyDictionary()
                .ToString(";", pair => Format.Format(pair.Key, pair.Value.ToString() ?? "null"));
        }
    }
}