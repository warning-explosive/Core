namespace SpaceEngineers.Core.GenericEndpoint.Conventions
{
    using System;
    using SettingsManager;
    using SettingsManager.Abstractions;

    /// <summary>
    /// Transport settings
    /// </summary>
    public class TransportSettings : IYamlSettings
    {
        /// <summary>
        /// RabbitMq connection string
        /// </summary>
        public RabbitMqConnectionString RabbitMqConnectionString { get; set; }

        /// <summary>
        /// Prefetch count
        /// TODO: description
        /// </summary>
        public ushort PrefetchCount { get; set; }

        /// <summary>
        /// Heartbeat interval
        /// TODO: description
        /// </summary>
        public TimeSpan HeartbeatInterval { get; set; }

        /// <summary>
        /// Network recovery interval
        /// TODO: description
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { get; set; }

        /// <summary>
        /// CircuitBreaker recovery interval
        /// TODO: description
        /// </summary>
        public TimeSpan CircuitBreakerRecoveryInterval { get; set; }
    }
}