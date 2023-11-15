namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Settings
{
    using System;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;

    /// <summary>
    /// Orm settings
    /// </summary>
    public class OrmSettings : ISettings
    {
        /// <summary> .cctor </summary>
        public OrmSettings()
        {
            CommandSecondsTimeout = 60;
        }

        /// <summary>
        /// Command timeout (seconds)
        /// </summary>
        public uint CommandSecondsTimeout { get; init; }

        /// <summary>
        /// Command timeout
        /// </summary>
        public TimeSpan CommandTimeout => TimeSpan.FromSeconds(CommandSecondsTimeout);
    }
}