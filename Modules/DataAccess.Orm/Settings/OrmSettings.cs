namespace SpaceEngineers.Core.DataAccess.Orm.Settings
{
    using System;
    using CrossCuttingConcerns.Settings;

    /// <summary>
    /// Orm settings
    /// </summary>
    public class OrmSettings : IYamlSettings
    {
        /// <summary> .cctor </summary>
        public OrmSettings()
        {
            QuerySecondsTimeout = 60;
        }

        /// <summary>
        /// Query timeout (seconds)
        /// </summary>
        public uint QuerySecondsTimeout { get; set; }

        /// <summary>
        /// Query timeout
        /// </summary>
        public TimeSpan QueryTimeout => TimeSpan.FromSeconds(QuerySecondsTimeout);
    }
}