namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Settings
{
    using System;
    using CrossCuttingConcerns.Settings;

    /// <summary>
    /// Outbox settings
    /// </summary>
    public class OutboxSettings : ISettings
    {
        /// <summary> .cctor </summary>
        public OutboxSettings()
        {
            OutboxDeliverySecondsInterval = 60;
        }

        /// <summary>
        /// Outbox delivery interval (seconds)
        /// </summary>
        public uint OutboxDeliverySecondsInterval { get; init; }

        /// <summary>
        /// Outbox delivery interval
        /// </summary>
        public TimeSpan OutboxDeliveryInterval => TimeSpan.FromSeconds(OutboxDeliverySecondsInterval);
    }
}