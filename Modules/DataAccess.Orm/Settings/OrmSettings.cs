namespace SpaceEngineers.Core.DataAccess.Orm.Settings
{
    using System;
    using CrossCuttingConcerns.Settings;

    /// <summary>
    /// Orm settings
    /// </summary>
    public class OrmSettings : ISettings
    {
        /// <summary> .cctor </summary>
        public OrmSettings()
        {
            DumpQueries = false;
            QuerySecondsTimeout = 60;
        }

        /// <summary>
        /// Dump queries
        /// </summary>
        public bool DumpQueries { get; set; }

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