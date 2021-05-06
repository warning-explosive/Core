namespace SpaceEngineers.Core.DataAccess.PostgreSql.Settings
{
    using System.Data;
    using SettingsManager.Abstractions;

    /// <summary>
    /// Postgre sql settings
    /// </summary>
    public class PostgreSqlSettings : IYamlSettings
    {
        /// <summary> .cctor </summary>
        public PostgreSqlSettings()
        {
            IsolationLevel = IsolationLevel.ReadCommitted;
        }

        /// <summary>
        /// Isolation level
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; }
    }
}