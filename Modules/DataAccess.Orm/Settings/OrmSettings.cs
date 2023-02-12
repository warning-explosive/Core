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
            CommandSecondsTimeout = 60;
        }

        /// <summary>
        /// Command timeout (seconds)
        /// </summary>
        public uint CommandSecondsTimeout { get; set; }

        /// <summary>
        /// Command timeout
        /// </summary>
        public TimeSpan CommandTimeout => TimeSpan.FromSeconds(CommandSecondsTimeout);
    }
}