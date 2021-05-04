namespace SpaceEngineers.Core.DataAccess.Orm.Settings
{
    using System;
    using System.Data;
    using SettingsManager.Abstractions;

    /// <summary>
    /// Orm settings
    /// </summary>
    public class OrmSettings : IYamlSettings
    {
        /// <summary> .cctor </summary>
        public OrmSettings()
        {
            QuerySecondsTimeout = 60;
            IsolationLevel = IsolationLevel.ReadCommitted;
        }

        /// <summary>
        /// Query timeout (seconds)
        /// </summary>
        public uint QuerySecondsTimeout { get; set; }

        /// <summary>
        /// Isolation level
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        /// Query timeout
        /// </summary>
        public TimeSpan QueryTimeout => TimeSpan.FromSeconds(QuerySecondsTimeout);
    }
}