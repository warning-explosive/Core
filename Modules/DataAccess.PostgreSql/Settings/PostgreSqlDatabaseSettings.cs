namespace SpaceEngineers.Core.DataAccess.PostgreSql.Settings
{
    using System.Data;
    using Basics;
    using CrossCuttingConcerns.Api.Abstractions;

    /// <summary>
    /// Postgre sql settings
    /// </summary>
    public class PostgreSqlDatabaseSettings : IYamlSettings
    {
        private const string Format = "{0}={1}";

        /// <summary> .cctor </summary>
        public PostgreSqlDatabaseSettings()
        {
            Host = "localhost";
            Port = 5432;
            Database = "SpaceEngineersDatabase";
            Schema = "public";

            /* TODO: use credentials vault */
            Username = "SpaceEngineer";
            Password = "1234";

            IsolationLevel = IsolationLevel.ReadCommitted;
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
        /// Schema
        /// </summary>
        public string Schema { get; set; }

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
                .ToString(";", pair => string.Format(Format, pair.Key, pair.Value));
        }
    }
}