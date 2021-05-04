namespace SpaceEngineers.Core.DataAccess.Orm.Settings
{
    using Basics;
    using SettingsManager.Abstractions;

    /// <summary>
    /// Database settings
    /// </summary>
    public class DatabaseSettings : IYamlSettings
    {
        private const string Format = "{0}={1}";

        /// <summary> .cctor </summary>
        public DatabaseSettings()
        {
            Host = "localhost";
            Port = 5432;
            Database = "SpaceEngineersDatabase";

            /* TODO: use credentials vault */
            Username = "SpaceEngineer";
            Password = "1234";
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
        /// Gets connection string
        /// </summary>
        /// <returns>Connection string</returns>
        public string GetConnectionString()
        {
            return this
                .ToPropertyDictionary()
                .ToString(";", pair => string.Format(Format, pair.Key, pair.Value));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return GetConnectionString();
        }
    }
}