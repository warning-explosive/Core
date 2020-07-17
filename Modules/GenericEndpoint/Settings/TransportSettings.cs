namespace SpaceEngineers.Core.GenericEndpoint.Settings
{
    using System;
    using SettingsManager.Abstractions;

    /// <summary>
    /// Transport settings
    /// </summary>
    public class TransportSettings : IYamlSettings
    {
        /// <summary> .cctor </summary>
        /// <param name="rabbitMqConnectionString">RabbitMqConnectionString</param>
        public TransportSettings(RabbitMqConnectionString rabbitMqConnectionString)
        {
            RabbitMqConnectionString = rabbitMqConnectionString;
            PrefetchCount = 8;
            HeartbeatInterval = TimeSpan.FromSeconds(15);
            NetworkRecoveryInterval = TimeSpan.FromSeconds(30);
            CircuitBreakerRecoveryInterval = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// RabbitMq connection string
        /// </summary>
        public RabbitMqConnectionString RabbitMqConnectionString { get; }

        /// <summary>
        /// Prefetch count
        /// TODO: description
        /// Default: 15 seconds
        /// </summary>
        public ushort PrefetchCount { get; set; }

        /// <summary>
        /// Heartbeat interval
        /// TODO: description
        /// Default: 15 seconds
        /// </summary>
        public TimeSpan HeartbeatInterval { get; set; }

        /// <summary>
        /// Network recovery interval
        /// TODO: description
        /// Default: 30 seconds
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { get; set; }

        /// <summary>
        /// CircuitBreaker recovery interval
        /// TODO: description
        /// Default: 30 seconds
        /// </summary>
        public TimeSpan CircuitBreakerRecoveryInterval { get; set; }
    }
}